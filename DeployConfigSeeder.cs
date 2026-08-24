using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Twenty57.Stadium.WebApp.Spa.Administration;
using Twenty57.Stadium.WebApp.Spa.Core.Entities;
using Twenty57.Stadium.WebApp.Spa.Core.Enums;
using Twenty57.Stadium.WebApp.Spa.Core.Interfaces;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Configuration;
using Twenty57.Stadium.WebApp.Spa.Core.Services.OAuth.ProviderSettings;

namespace Twenty57.Stadium.WebApp.Spa.Template
{
	// Container-only startup seeder. The user fills the exported deploy-config.json and deployment injects
	// the filled copy as the DeployConfig env var (JSON); this applies it to the running container:
	internal static class DeployConfigSeeder
	{
		public const string DeployConfigEnvName = "DeployConfig";
		private const string AuditLogSource = "Deployment";

		private static readonly (string Constant, Func<OAuthSection, string> Selector)[] OAuthSettingMap =
		{
			(AuthenticationSetting.AuthenticationAuthorityUri, o => o.Authority),
			(AuthenticationSetting.ClientID,          o => o.ClientId),
			(AuthenticationSetting.ApiResourceName,   o => o.ApiResourceName),
			(AuthenticationSetting.ApiResourceSecret, o => o.ApiResourceSecret),
			(AuthenticationSetting.Audience,          o => o.Audience),
			(AuthenticationSetting.Scopes,            o => o.Scopes),
			(AuthenticationSetting.RoleClaimName,     o => o.RoleClaimName),
			(AuthenticationSetting.RedirectUrl,       o => o.RedirectUrl),
			(AuthenticationSetting.LogoutRedirectUrl, o => o.LogoutRedirectUrl),
			(AuthenticationSetting.Tenant,            o => o.Tenant),
			(AuthenticationSetting.OidcProvider,      o => o.OidcProvider),
		};

		public static void Seed(WebApplication app)
		{
			string volumeDirectory = ConfigStorageLocation.VolumeDirectory;
			if (string.IsNullOrEmpty(volumeDirectory))
				return;   // Windows/IIS

			string manifestJson = Environment.GetEnvironmentVariable(DeployConfigEnvName);
			if (string.IsNullOrWhiteSpace(manifestJson))
				return;

			try
			{
				var manifest = JsonConvert.DeserializeObject<Manifest>(manifestJson);
				if (manifest == null)
					return;

				AuthenticationType? mode = ResolveMode(manifest);

				using (var scope = app.Services.CreateScope())
				{
					IDictionary<string, string> oauthSettings = ApplyDatabaseValues(scope.ServiceProvider, manifest, mode);

					// OAuth login mode is persisted BEFORE the admin step: admin validation reads AuthenticationType
					// from the on-disk appsettings and an OAuth admin has no password.
					if (mode == AuthenticationType.OAuth)
						ApplyLoginMode(app, AuthenticationType.OAuth, oauthSettings);

					ApplyAdminUser(scope.ServiceProvider, manifest, mode).GetAwaiter().GetResult();
				}

				// Cookie/Anonymous applied last: this is the only step that persists to the volume without a
				// rollback, so it runs only after the DB + admin steps succeed - a failed deploy must not leave a
				// more-permissive login mode persisted for a later no-manifest boot to pick up.
				if (mode == AuthenticationType.Cookie || mode == AuthenticationType.Anonymous)
					ApplyLoginMode(app, mode.Value, oauthSettings: null);
			}
			catch (Exception exception)
			{
				app.Logger.LogError(
					exception,
					"Failed to apply the {EnvName} deploy-config manifest. Check the manifest JSON and its values (e.g. the admin password policy), then redeploy.",
					DeployConfigEnvName);
				throw;
			}
		}

		private static AuthenticationType? ResolveMode(Manifest manifest)
		{
			string modeName = manifest.Authentication?.Mode;

			if (string.IsNullOrWhiteSpace(modeName))
				return null;

			// Match by NAME only. Not integers (e.g. "0" -> Anonymous)
			bool isKnownName = Enum.GetNames<AuthenticationType>()
									.Any(name => name.Equals(modeName, StringComparison.OrdinalIgnoreCase));
			if (isKnownName)
			{
				var mode = Enum.Parse<AuthenticationType>(modeName, ignoreCase: true);
				if (mode == AuthenticationType.Cookie || mode == AuthenticationType.Anonymous || mode == AuthenticationType.OAuth)
					return mode;
			}

			throw new InvalidDataException(
				$"Unsupported authentication mode '{modeName}' in the deploy-config manifest. Supported modes: Cookie, Anonymous, OAuth.");
		}

		private static IDictionary<string, string> ApplyDatabaseValues(IServiceProvider services, Manifest manifest, AuthenticationType? mode)
		{
			var context = services.GetRequiredService<AdministrationContext>();

			MergeValues(manifest.Settings, context.Settings, s => s.Value, (s, value) => s.Value = value);
			MergeValues(manifest.Connections, context.Connections, c => c.ConnectionString, (c, value) => c.ConnectionString = value);

			IDictionary<string, string> oauthSettings = null;
			if (mode == AuthenticationType.OAuth)
				oauthSettings = ApplyOAuthSettings(context, manifest.Authentication?.OAuth);

			context.SaveChanges();
			return oauthSettings;
		}

		// A null incoming value is SKIPPED: the manifest did not supply it
		// A supplied value (incl. "") becomes the DefaultValue
		// A runtime edit (the stored value diverged from DefaultValue) is preserved
		// An un-edited value is updated to the manifest value
		private static void MergeValues<TEntity>(
			NameValue[] incoming,
			IQueryable<TEntity> entities,
			Func<TEntity, string> getValue,
			Action<TEntity, string> setValue)
			where TEntity : ASettingBase
		{
			foreach (var item in incoming ?? Enumerable.Empty<NameValue>())
			{
				if (item.Value == null)
					continue;

				var entity = entities.FirstOrDefault(e => e.Name == item.Name);
				if (entity == null)
					continue;

				bool isEdited = getValue(entity) != entity.DefaultValue;
				entity.DefaultValue = item.Value;
				if (!isEdited)
					setValue(entity, item.Value);
			}
		}

		private static IDictionary<string, string> ApplyOAuthSettings(AdministrationContext context, OAuthSection oauth)
		{
			oauth ??= new OAuthSection();

			var existing = context.AuthenticationSettings.ToDictionary(s => s.Name, s => s.Value);

			var merged = new Dictionary<string, string>();
			foreach (var (constant, selector) in OAuthSettingMap)
			{
				string incoming = selector(oauth);
				merged[constant] = incoming ?? (existing.TryGetValue(constant, out string current) ? current : string.Empty);
			}

			context.AuthenticationSettings.RemoveRange(context.AuthenticationSettings);
			context.AuthenticationSettings.AddRange(merged.Select(kvp => new AuthenticationSetting(kvp.Key, kvp.Value)));

			return merged;
		}

		private static async Task ApplyAdminUser(IServiceProvider services, Manifest manifest, AuthenticationType? mode)
		{
			var admin = manifest.Admin;
			if (admin == null)
				return;

			var userAdministration = services.GetRequiredService<IUserAdministration>();

			if (mode == AuthenticationType.OAuth)
			{
				string email = string.IsNullOrEmpty(admin.Email) ? null : admin.Email;
				string uniqueId = string.IsNullOrEmpty(admin.UniqueId) ? null : admin.UniqueId;

				if (email == null && uniqueId == null)
					return;

				UserInfo existingOAuthUser = null;
				if (uniqueId != null)
					userAdministration.TryGetUserByUniqueId(uniqueId, out existingOAuthUser);
				if (existingOAuthUser == null && email != null)
					userAdministration.TryGetUserByEmail(email, out existingOAuthUser);

				if (existingOAuthUser != null)
				{
					await userAdministration.EditUserAsync(
						id: existingOAuthUser.User.Id,
						isAdministrator: true,
						auditLogSource: AuditLogSource
					);
				}
				else
				{
					await userAdministration.AddUserAsync(
						email: email,
						name: email ?? uniqueId,
						userName: email ?? uniqueId,
						password: null,
						isAdministrator: true,
						roles: [Role.DefaultRoleName],
						uniqueId: uniqueId,
						auditLogSource: AuditLogSource
					);
				}

				return;
			}

			if (string.IsNullOrEmpty(admin.Email) || string.IsNullOrEmpty(admin.Password))
				return;

			if (userAdministration.TryGetUserByEmail(admin.Email, out var existingUser))
			{
				await userAdministration.EditUserAsync(
					id: existingUser.User.Id,
					password: admin.Password,
					isAdministrator: true,
					auditLogSource: AuditLogSource
				);
			}
			else
			{
				await userAdministration.AddUserAsync(
					email: admin.Email,
					name: admin.Email,
					userName: admin.Email,
					password: admin.Password,
					isAdministrator: true,
					roles: [Role.DefaultRoleName],
					auditLogSource: AuditLogSource
				);
			}
		}

		private static void ApplyLoginMode(WebApplication app, AuthenticationType mode, IDictionary<string, string> oauthSettings)
		{
			AppSettingsHelper.ExecuteAndSave(a =>
			{
				a.AppSettings.Config.AuthenticationType = mode;

				if (mode == AuthenticationType.OAuth)
				{
					var oidcSettings = OidcSettingsProvider.CreateInstance(oauthSettings).Settings;
					a.AppSettings.Config.OAuth.Authority = oidcSettings.Authority;
					a.AppSettings.Config.OAuth.Audience = oidcSettings.Audience;
					a.AppSettings.Config.OAuth.ClientId = oidcSettings.ClientId;
					a.AppSettings.Config.OAuth.ApiResourceName = oidcSettings.ApiResourceName;
					a.AppSettings.Config.OAuth.ApiResourceSecret = oidcSettings.ApiResourceSecret;
					a.AppSettings.Config.OAuth.Scopes = oidcSettings.Scopes;
				}
			});

			if (app.Configuration is IConfigurationRoot configurationRoot)
				configurationRoot.Reload();
		}

		private class Manifest
		{
			public AuthenticationSection Authentication { get; set; }
			public AdminSection Admin { get; set; }
			public NameValue[] Settings { get; set; }
			public NameValue[] Connections { get; set; }
		}

		private class AuthenticationSection
		{
			public string Mode { get; set; }
			public OAuthSection OAuth { get; set; }
		}

		private class OAuthSection
		{
			public string Authority { get; set; }
			public string ClientId { get; set; }
			public string ApiResourceName { get; set; }
			public string ApiResourceSecret { get; set; }
			public string Audience { get; set; }
			public string Scopes { get; set; }
			public string RoleClaimName { get; set; }
			public string RedirectUrl { get; set; }
			public string LogoutRedirectUrl { get; set; }
			public string Tenant { get; set; }
			public string OidcProvider { get; set; }
		}

		private class AdminSection
		{
			public string Email { get; set; }
			public string Password { get; set; }
			public string UniqueId { get; set; }
		}

		private class NameValue
		{
			public string Name { get; set; }
			public string Value { get; set; }
		}
	}
}
