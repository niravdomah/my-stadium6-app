using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Twenty57.Stadium.WebApp.Spa.Core.Entities;
using Twenty57.Stadium.WebApp.Spa.Core.Enums;
using Twenty57.Stadium.WebApp.Spa.Core.Interfaces;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Configuration;
using Twenty57.Stadium.WebApp.Spa.Core.Services.OAuth.ProviderSettings;
using Twenty57.Stadium.WebApp.Spa.Template.Areas.WebApi.Models;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.WebApi.Controllers
{
	[ApiController]
	[Area("WebApi")]
	[Route("api/login-mode")]
	[AllowAnonymous]
	[WebApiKeyValidation]
	public class LoginModeController : ControllerBase
	{
		private const string auditLogSource = "SAM Authentication";

		private readonly IUserAdministration userAdministration;
		private readonly IAuthenticationSettingStore authenticationSettingStore;

		public LoginModeController(
			IUserAdministration userAdministration,
			IAuthenticationSettingStore authenticationSettingStore)
		{
			this.userAdministration = userAdministration;
			this.authenticationSettingStore = authenticationSettingStore;
		}

		// GET: api/login-mode?key=key
		[HttpGet]
		public IActionResult GetLoginMode(string key)
		{
			var authenticationType = AppSettingsHelper.GetInstance().AppSettings.Config.AuthenticationType;
			return Ok((int)authenticationType);
		}

		// GET: api/login-mode/email-exists?key=key&email=email
		[HttpGet]
		[Route("email-exists")]
		public async Task<IActionResult> EmailExists(string key, string email)
		{
			return Ok(await this.userAdministration.EmailExistsAsync(email));
		}

		// GET: api/login-mode/authentication-settings?key=key
		[HttpGet]
		[Route("authentication-settings")]
		public IActionResult AuthenticationSettings(string key)
		{
			return Ok(this.authenticationSettingStore.AuthenticationSettings.ToArray());
		}

		// PUT: api/login-mode?key=key
		[HttpPut]
		public async Task<IActionResult> EditLoginMode(string key, LoginModeInfo loginModeInfo)
		{
			switch (loginModeInfo.NewSecurityMode)
			{
				case AuthenticationType.Anonymous: SetAuthenticationType(loginModeInfo.NewSecurityMode); break;
				case AuthenticationType.Cookie:
					{
						if (string.IsNullOrEmpty(loginModeInfo.Email) && !string.IsNullOrEmpty(loginModeInfo.Password))
							throw new Exception("The Email field is required.");

						bool emailExists = !string.IsNullOrEmpty(loginModeInfo.Email) && await this.userAdministration.EmailExistsAsync(loginModeInfo.Email);

						if (!emailExists && !string.IsNullOrEmpty(loginModeInfo.Email) && string.IsNullOrEmpty(loginModeInfo.Password))
							throw new Exception("The Password field is required.");

						SetAuthenticationType(loginModeInfo.NewSecurityMode);

						if (emailExists)
						{
							var userInfo = await this.userAdministration.GetUserByEmailAsync(loginModeInfo.Email);
							await this.userAdministration.EditUserAsync(
								id: userInfo.User.Id,
								email: userInfo.User.Email,
								name: loginModeInfo.Name,
								userName: loginModeInfo.UserName,
								isAdministrator: true,
								password: loginModeInfo.Password,
								auditLogSource: auditLogSource,
								auditChangedByUserId: null
							);
						}
						else if (loginModeInfo.Email != null && loginModeInfo.Password != null)
						{
							await this.userAdministration.AddUserAsync(
								email: loginModeInfo.Email,
								name: loginModeInfo.Name,
								userName: loginModeInfo.UserName,
								password: loginModeInfo.Password,
								isAdministrator: true,
								roles: [Role.DefaultRoleName],
								auditLogSource: auditLogSource,
								auditChangedByUserId: null
							);
						}

						break;
					}
				case AuthenticationType.OAuth:
					{
						await this.authenticationSettingStore.UpdateAllAsync(
							[
								new AuthenticationSetting(AuthenticationSetting.RedirectUrl, loginModeInfo.RedirectUrl),
								new AuthenticationSetting(AuthenticationSetting.LogoutRedirectUrl, loginModeInfo.LogoutRedirectUrl),
								new AuthenticationSetting(AuthenticationSetting.OidcProvider, loginModeInfo.OidcProvider.ToString()),
								new AuthenticationSetting(AuthenticationSetting.AuthenticationAuthorityUri, loginModeInfo.AuthenticationAuthorityUri),
								new AuthenticationSetting(AuthenticationSetting.Tenant, loginModeInfo.Tenant),
								new AuthenticationSetting(AuthenticationSetting.ClientID, loginModeInfo.ClientID),
								new AuthenticationSetting(AuthenticationSetting.ApiResourceName, loginModeInfo.ApiResourceName),
								new AuthenticationSetting(AuthenticationSetting.ApiResourceSecret, loginModeInfo.ApiResourceSecret),
								new AuthenticationSetting(AuthenticationSetting.RoleClaimName, loginModeInfo.RoleClaimName),
								new AuthenticationSetting(AuthenticationSetting.Audience, loginModeInfo.Audience),
								new AuthenticationSetting(AuthenticationSetting.Scopes, loginModeInfo.Scopes)
							]
						);

						var oidcSettings = OidcSettingsProvider.CreateInstance(this.authenticationSettingStore.AuthenticationSettings.ToDictionary(s => s.Name, s => s.Value))
																.Settings;
						AppSettingsHelper.ExecuteAndSave(s =>
						{
							s.AppSettings.Config.OAuth.Authority = oidcSettings.Authority;
							s.AppSettings.Config.OAuth.Audience = oidcSettings.Audience;
							s.AppSettings.Config.OAuth.ClientId = oidcSettings.ClientId;
							s.AppSettings.Config.OAuth.ApiResourceName = oidcSettings.ApiResourceName;
							s.AppSettings.Config.OAuth.ApiResourceSecret = oidcSettings.ApiResourceSecret;
							s.AppSettings.Config.OAuth.Scopes = oidcSettings.Scopes;
						});
						SetAuthenticationType(loginModeInfo.NewSecurityMode);

						bool hasUniqueId = !string.IsNullOrEmpty(loginModeInfo.UniqueId);
						bool hasEmail = !string.IsNullOrEmpty(loginModeInfo.Email);

						if ((hasUniqueId && this.userAdministration.TryGetUserByUniqueId(loginModeInfo.UniqueId, out var userInfo))
							|| (hasEmail && this.userAdministration.TryGetUserByEmail(loginModeInfo.Email, out userInfo)))
						{
							if (!userInfo.User.IsAdministrator)
							{
								await this.userAdministration.EditUserAsync(
									id: userInfo.User.Id,
									isAdministrator: true,
									auditLogSource: auditLogSource,
									auditChangedByUserId: null
								);
							}

							if (hasEmail && (userInfo.User.Email != loginModeInfo.Email))
							{
								await this.userAdministration.EditUserAsync(
									id: userInfo.User.Id,
									email: loginModeInfo.Email,
									auditLogSource: auditLogSource,
									auditChangedByUserId: null
								);
							}

							if (hasUniqueId && (userInfo.User.UniqueId != loginModeInfo.UniqueId))
							{
								await this.userAdministration.EditUserAsync(
									id: userInfo.User.Id,
									uniqueId: loginModeInfo.UniqueId,
									auditLogSource: auditLogSource,
									auditChangedByUserId: null
								);
							}

							if (userInfo.User.Name != loginModeInfo.Name)
							{
								await this.userAdministration.EditUserAsync(
									id: userInfo.User.Id,
									name: loginModeInfo.Name,
									auditLogSource: auditLogSource,
									auditChangedByUserId: null
								);
							}

						}
						else if (hasUniqueId || hasEmail)
						{
							await this.userAdministration.AddUserAsync(
									email: loginModeInfo.Email,
									name: loginModeInfo.Name,
									userName: null,
									password: null,
									isAdministrator: true,
									roles: [Role.DefaultRoleName],
									uniqueId: loginModeInfo.UniqueId,
									auditLogSource: auditLogSource,
									auditChangedByUserId: null
								);
						}

						break;
					}
			}

			return Ok();
		}

		private void SetAuthenticationType(AuthenticationType authenticationType)
		{
			AppSettingsHelper.ExecuteAndSave(s => s.AppSettings.Config.AuthenticationType = authenticationType);
		}
	}
}
