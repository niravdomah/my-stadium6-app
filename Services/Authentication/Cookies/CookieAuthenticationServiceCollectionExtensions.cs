using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Threading.Tasks;
using Twenty57.Stadium.WebApp.Spa.Administration.Stores;
using Twenty57.Stadium.WebApp.Spa.Core.Entities;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Configuration;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.Authentication.Cookies
{
	public static class CookieAuthenticationServiceCollectionExtensions
	{
		private const string SessionTimeoutHeaderName = "cookie-authentication-timeout";

		public static IServiceCollection AddCookieAuthentication(this IServiceCollection services)
		{
			services.AddIdentity<Core.Entities.User, Role>()
					.AddUserStore<UserStore>()
					.AddRoleStore<RoleStore>()
					.AddClaimsPrincipalFactory<IdentityUserClaimsPrincipalFactory>()
					.AddDefaultTokenProviders();

			services.Replace(ServiceDescriptor.Scoped<IUserValidator<Core.Entities.User>, CustomUserValidator<Core.Entities.User>>());

			services.Configure<IdentityOptions>(o =>
			{
				o.Lockout.AllowedForNewUsers = false;

				o.Password.RequireDigit = false;
				o.Password.RequireLowercase = false;
				o.Password.RequireNonAlphanumeric = false;
				o.Password.RequireUppercase = false;
				o.Password.RequiredLength = 0;

				o.User.AllowedUserNameCharacters = null;
				o.User.RequireUniqueEmail = true;
			});

			services.AddOptions<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme)
					.Configure<IOptionsMonitor<Config>>((o, c) =>
					{
						o.Cookie.SameSite = SameSiteMode.Strict;
						o.Events = new CookieAuthenticationEvents
						{
							OnRedirectToLogin = redirectContext =>
							{
								redirectContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
								return Task.CompletedTask;
							},
							OnRedirectToAccessDenied = redirectContext =>
							{
								redirectContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
								return Task.CompletedTask;
							},
							OnValidatePrincipal = cookieValidatePrincipalContext =>
							{
								int sessionStateTimeoutInMs = c.CurrentValue.SessionStateTimeout * 60 * 1000;
								cookieValidatePrincipalContext.Response.Headers.Append(SessionTimeoutHeaderName, sessionStateTimeoutInMs.ToString());
								return Task.CompletedTask;
							}
						};
						o.ExpireTimeSpan = TimeSpan.FromMinutes(c.CurrentValue.SessionStateTimeout);
						c.OnChange(config => o.ExpireTimeSpan = TimeSpan.FromMinutes(c.CurrentValue.SessionStateTimeout));
					});

			services.AddOptions<DataProtectionTokenProviderOptions>()
				.Configure<IOptionsMonitor<Config>>((o, c) =>
				{
					o.TokenLifespan = TimeSpan.FromMinutes(c.CurrentValue.PasswordTokenTimeout);
					c.OnChange(config => o.TokenLifespan = TimeSpan.FromMinutes(c.CurrentValue.PasswordTokenTimeout));
				});

			return services;
		}
	}
}
