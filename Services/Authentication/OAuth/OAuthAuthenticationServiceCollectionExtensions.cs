using IdentityModel.AspNetCore.OAuth2Introspection;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Serilog;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Twenty57.Stadium.WebApp.Spa.Core.Enums;
using Twenty57.Stadium.WebApp.Spa.Core.Interfaces;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Configuration;
using Twenty57.Stadium.WebApp.Spa.Core.Services.OAuth.ProviderSettings;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.Authentication.OAuth
{
	public static class OAuthAuthenticationServiceCollectionExtensions
	{
		private const string TokenIntrospectionScheme = "introspection";

		public static IServiceCollection AddOAuthAuthentication(this IServiceCollection services)
		{
			services.AddMemoryCache();
			services.AddScoped(p => ActivatorUtilities.CreateInstance<OidcSettingsProvider>(p));

			services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
					.Configure<IOptionsMonitor<Config>>((o, optionsMonitor) =>
					{
						var oidcSettings = optionsMonitor.CurrentValue.OAuth;
						o.Authority = oidcSettings.Authority;
						o.Audience = oidcSettings.Audience;
						o.TokenValidationParameters.ValidateAudience = !string.IsNullOrEmpty(oidcSettings.Audience);
						o.ForwardDefaultSelector = Selector.ForwardReferenceToken(TokenIntrospectionScheme);
						o.UseSecurityTokenValidators = true;

						optionsMonitor.OnChange(config =>
						{
							var newOidcSettings = config.OAuth;
							o.Authority = newOidcSettings.Authority;
							o.Audience = newOidcSettings.Audience;
							o.TokenValidationParameters.ValidateAudience = !string.IsNullOrEmpty(newOidcSettings.Audience);
							o.ConfigurationManager.RequestRefresh();
						});

						o.Events = new JwtBearerEvents()
						{
							OnTokenValidated = async c =>
							{
								var bearerToken = (JwtSecurityToken)c.SecurityToken;
								var serviceProvider = c.HttpContext.RequestServices;

								var userAdministration = serviceProvider.GetRequiredService<IUserAdministration>();
								var userInfo = await GetUserInfoAsync(bearerToken, serviceProvider, o.ConfigurationManager);

								var claims = ClaimsHelper.BuildClaims(userInfo, userAdministration);

								c.Principal.AddIdentity(new ClaimsIdentity(claims, authenticationType: "OAuth"));
							}
						};
					});

			services.AddOptions<OAuth2IntrospectionOptions>(TokenIntrospectionScheme)
					.Configure<IOptionsMonitor<Config>>((o, optionsMonitor) =>
					{
						o.EnableCaching = true;

						var oidcSettings = optionsMonitor.CurrentValue.OAuth;
						o.Authority = oidcSettings.Authority.ToLower();
						o.ClientId = oidcSettings.ApiResourceName;
						o.ClientSecret = oidcSettings.ApiResourceSecret;

						optionsMonitor.OnChange(config =>
						{
							var newOidcSettings = config.OAuth;
							o.Authority = newOidcSettings.Authority.ToLower();
							o.ClientId = newOidcSettings.ApiResourceName;
							o.ClientSecret = newOidcSettings.ApiResourceSecret;
						});

						o.Events = new OAuth2IntrospectionEvents()
						{
							OnTokenValidated = async c =>
							{
								var serviceProvider = c.HttpContext.RequestServices;

								var userInfo = new UserInfo
								{
									UniqueId = c.Principal.FindFirstValue("sub"),
									Email = c.Principal.FindFirstValue("email"),
									Name = c.Principal.FindFirstValue("name")
								};

								if (userInfo.UniqueId == null || userInfo.Email == null)
								{
									string referenceToken = c.SecurityToken;
									userInfo = await GetUserInfoAsync(referenceToken, serviceProvider);
								}

								var userAdministration = serviceProvider.GetRequiredService<IUserAdministration>();
								var claims = ClaimsHelper.BuildClaims(userInfo, userAdministration);

								c.Principal.AddIdentity(new ClaimsIdentity(claims, authenticationType: "OAuth"));
							}
						};
					});

			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
					.AddJwtBearer()
					.AddOAuth2Introspection(TokenIntrospectionScheme);

			return services;
		}

		private static async Task<UserInfo> GetUserInfoAsync(JwtSecurityToken bearerToken, IServiceProvider serviceProvider, IConfigurationManager<OpenIdConnectConfiguration> configurationManager)
		{
			var oidcSettings = serviceProvider.GetRequiredService<OidcSettingsProvider>()
												.Settings;

			var userInfoEntity = new UserInfo();

			if (oidcSettings.OidcProvider is OidcProvider.Google or OidcProvider.AzureAD)
			{
				userInfoEntity.UniqueId = bearerToken.Subject;
				userInfoEntity.Email = GetEmail(claimType => bearerToken.Payload.TryGetValue(claimType, out object claimValue) && claimValue is string claim ? claim : null);
				userInfoEntity.Name = (string)bearerToken.Payload["name"];

				if (oidcSettings.RoleClaimName != null && bearerToken.Payload.TryGetValue(oidcSettings.RoleClaimName, out object roles))
					userInfoEntity.Roles = ((JsonElement)roles).EnumerateArray().Select(r => r.GetString()).ToList();
			}
			else
			{
				var memoryCache = serviceProvider.GetService<IMemoryCache>();
				var userInfo = await memoryCache.GetOrCreateAsync(bearerToken.RawData, async cacheEntry =>
				{
					cacheEntry.SlidingExpiration = TimeSpan.FromMinutes(30);

					var oidcConfig = await configurationManager.GetConfigurationAsync(cancel: default);
					var httpClient = serviceProvider.GetService<IHttpClientFactory>().CreateClient();

					return await GetUserInfoAsync(httpClient, bearerToken, oidcConfig);
				});

				userInfoEntity.UniqueId = userInfo["sub"]?.GetValue<string>();
				userInfoEntity.Email = GetEmail(claimType => userInfo[claimType] is JsonValue claimValue && claimValue.TryGetValue(out string claim) ? claim : null);
				userInfoEntity.Name = userInfo["name"]?.GetValue<string>();

				if (oidcSettings.RoleClaimName != null && userInfo[oidcSettings.RoleClaimName] is JsonArray roles)
					userInfoEntity.Roles = roles.Select(r => r.GetValue<string>()).ToList();
			}

			return userInfoEntity;
		}

		private static async Task<JsonNode> GetUserInfoAsync(HttpClient httpClient, JwtSecurityToken bearerToken, OpenIdConnectConfiguration oidcConfig)
		{
			var httpRequest = new HttpRequestMessage(HttpMethod.Get, oidcConfig.UserInfoEndpoint);
			httpRequest.Headers.Add("Authorization", $"Bearer {bearerToken.RawData}");

			var response = await httpClient.SendAsync(httpRequest);
			string content = await response.Content.ReadAsStringAsync();
			return JsonSerializer.Deserialize<JsonNode>(content);
		}

		private static async Task<UserInfo> GetUserInfoAsync(string referenceToken, IServiceProvider serviceProvider)
		{
			var memoryCache = serviceProvider.GetService<IMemoryCache>();
			var oidcSettings = serviceProvider.GetRequiredService<OidcSettingsProvider>()
												.Settings;

			var userInfo = await memoryCache.GetOrCreateAsync(referenceToken, async cacheEntry =>
			{
				cacheEntry.SlidingExpiration = TimeSpan.FromMinutes(5);

				string userInfoIntrospectionEndpoint = await GetUserInfoIntrospectionEndpointAsync(serviceProvider);
				var httpClient = serviceProvider.GetService<IHttpClientFactory>().CreateClient();

				return await GetUserInfoAsync(httpClient, referenceToken, userInfoIntrospectionEndpoint);
			});

			var userInfoEntity = new UserInfo
			{
				UniqueId = userInfo["sub"]?.GetValue<string>(),
				Email = userInfo["email"]?.GetValue<string>(),
				Name = userInfo["name"]?.GetValue<string>()
			};

			if (oidcSettings.RoleClaimName != null && userInfo[oidcSettings.RoleClaimName] is JsonArray roles)
				userInfoEntity.Roles = roles.Select(r => r.GetValue<string>()).ToList();

			return userInfoEntity;
		}

		private static async Task<JsonNode> GetUserInfoAsync(HttpClient httpClient, string referenceToken, string userInfoIntrospectionEndpoint)
		{
			var httpRequest = new HttpRequestMessage(HttpMethod.Get, userInfoIntrospectionEndpoint);
			httpRequest.Headers.Add("Authorization", $"Bearer {referenceToken}");

			var response = await httpClient.SendAsync(httpRequest);
			string content = await response.Content.ReadAsStringAsync();
			return JsonSerializer.Deserialize<JsonNode>(content);
		}

		private static async Task<string> GetUserInfoIntrospectionEndpointAsync(IServiceProvider serviceProvider)
		{
			var client = serviceProvider.GetService<IHttpClientFactory>().CreateClient("IdentityModel.AspNetCore.OAuth2Introspection.BackChannelHttpClientName");
			var oidcSettings = serviceProvider.GetRequiredService<OidcSettingsProvider>()
												.Settings;

			var discoveryResponse = await client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
			{
				Address = oidcSettings.Authority.ToLower(),
				Policy = new DiscoveryPolicy()
			});

			return discoveryResponse.UserInfoEndpoint;
		}

		private static string GetEmail(Func<string, string> getEmailClaim)
		{
			string email = getEmailClaim("email");
			if (!string.IsNullOrEmpty(email))
				return email;

			string preferredUsername = getEmailClaim("preferred_username");
			if (!string.IsNullOrEmpty(preferredUsername) && IsEmail(preferredUsername))
			{
				Log.ForContext<Startup>()
					.Information("Email claim is missing, using preferred_username claim instead.");

				return preferredUsername;
			}

			throw new Exception($"SSO Authentication failed: Email claim is missing.");
		}


		private static bool IsEmail(string value)
		{
			if (string.IsNullOrEmpty(value))
				return false;

			string validEmailPattern = @"^[a-zA-Z0-9.!#$%&'*+\/=?^_`{|}~-]{1,64}@[a-zA-Z0-9-]{1,64}\.[\.a-zA-Z0-9-]{2,64}$";

			return Regex.IsMatch(value, validEmailPattern);
		}
	}
}
