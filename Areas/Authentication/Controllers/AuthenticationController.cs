using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Twenty57.Stadium.WebApp.Spa.Core.Entities;
using Twenty57.Stadium.WebApp.Spa.Core.Enums;
using Twenty57.Stadium.WebApp.Spa.Core.Interfaces;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Configuration;
using Twenty57.Stadium.WebApp.Spa.Core.Services.OAuth.ProviderSettings;
using Twenty57.Stadium.WebApp.Spa.Template.Areas.Authentication.Models;
using Twenty57.Stadium.WebApp.Spa.Template.Controllers;
using Twenty57.Stadium.WebApp.Spa.Template.Extensions;
using Twenty57.Stadium.WebApp.Spa.Template.Services.Authentication;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Authentication.Controllers
{
	[ApiController]
	[Area("Authentication")]
	[Route("api/[controller]/[action]")]
	public class AuthenticationController : AValidatedControllerBase
	{
		private const string LoginFailedMessage = "Email and password not found";

		private readonly UserManager<User> userManager;
		private readonly SignInManager<User> signInManager;
		private readonly IUserService userService;
		private readonly IUserAdministration userAdministration;
		private readonly GenericProviderSettings oidcSettings;
		private readonly IAntiforgery antiforgery;
		private readonly Config config;
		private readonly ISessionFilesJanitor sessionFilesJanitor;
		private readonly ILookupNormalizer keyNormalizer;

		public AuthenticationController(
			UserManager<User> userManager,
			SignInManager<User> signInManager,
			IUserService userService,
			IUserAdministration userAdministration,
			OidcSettingsProvider oidcSettingsProvider,
			IAntiforgery antiforgery,
			IOptionsSnapshot<Config> configOptions,
			ISessionFilesJanitor sessionFilesJanitor,
			ILookupNormalizer keyNormalizer)
		{
			this.userManager = userManager;
			this.signInManager = signInManager;
			this.userService = userService;
			this.userAdministration = userAdministration;
			this.oidcSettings = oidcSettingsProvider.Settings;
			this.antiforgery = antiforgery;
			this.config = configOptions.Value;
			this.sessionFilesJanitor = sessionFilesJanitor;
			this.keyNormalizer = keyNormalizer;
		}

		// GET: api/Authentication/State
		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> State()
		{
			var authenticationState = new AuthenticationState
			{
				AuthenticationType = this.config.AuthenticationType
			};

			if (this.config.AuthenticationType == AuthenticationType.OAuth)
			{
				authenticationState.AuthenticationSettings = new AuthenticationSettings
				{
					OidcProvider = this.oidcSettings.OidcProvider.ToString(),
					Audience = this.oidcSettings.Audience,
					Authority = this.oidcSettings.Authority,
					Tenant = this.oidcSettings.Tenant,
					RoleClaimName = this.oidcSettings.RoleClaimName,
					ResponseType = this.oidcSettings.ResponseType,
					RedirectUrl = this.oidcSettings.RedirectUrl,
					LogoutRedirectUrl = this.oidcSettings.LogoutRedirectUrl,
					ClientId = this.oidcSettings.ClientId,
					Scope = this.oidcSettings.Scopes,
					EndSessionEndpoint = this.oidcSettings.EndSessionEndpoint
				};
			}

			authenticationState.UserClaims = await this.userService.GetLoggedInUserClaimsAsync();

			return Ok(authenticationState);
		}

		// GET: api/Authentication/CheckValidSession
		[HttpGet]
		public IActionResult CheckValidSession()
		{
			return NoContent();
		}

		// POST: api/Authentication/RefreshOidcUser
		[HttpPost]
		public async Task<IActionResult> RefreshOidcUser()
		{
			if (User != null && User.Identity.IsAuthenticated)
			{
				const string auditLogSource = "Web App Authentication";

				string uniqueId = User.Claims.Single(c => c.Type == CustomClaimTypes.UniqueId).Value;
				string email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
				string name = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
				string auditChangedByUserId = this.GetLoggedInUserId();

				var userEntity = this.userManager.Users.SingleOrDefault(u => u.UniqueId == uniqueId || (email != null && u.NormalizedEmail == this.keyNormalizer.NormalizeEmail(email)));
				if (userEntity == null)
				{
					await this.userAdministration.AddUserAsync(
						email: email,
						name: name,
						userName: null,
						password: null,
						isAdministrator: false,
						roles: [Role.DefaultRoleName],
						uniqueId: uniqueId,
						auditLogSource: auditLogSource,
						auditChangedByUserId: auditChangedByUserId
					);
				}
				else
				{
					if (!string.IsNullOrEmpty(userEntity.UniqueId) && userEntity.UniqueId != uniqueId)
						throw new Exception($"{userEntity.Email} is already associated with another user in Stadium. Please contact the system administrator.");

					if (userEntity.Name != name)
					{
						await this.userAdministration.EditUserAsync(
							id: userEntity.Id,
							name: name,
							auditLogSource: auditLogSource,
							auditChangedByUserId: auditChangedByUserId
						);
					}

					if (string.IsNullOrEmpty(userEntity.UniqueId))
					{
						await this.userAdministration.EditUserAsync(
							id: userEntity.Id,
							uniqueId: uniqueId,
							auditLogSource: auditLogSource,
							auditChangedByUserId: auditChangedByUserId
						);
					}
				}
			}

			return NoContent();
		}

		// POST: api/Authentication/RenewAntiforgeryToken
		[HttpPost]
		[AllowAnonymous]
		public IActionResult RenewAntiforgeryToken()
		{
			if (this.config.AuthenticationType == AuthenticationType.Anonymous)
				return NoContent();

			var tokens = this.antiforgery.GetAndStoreTokens(HttpContext);
			Response.Cookies.Append(
				AntiforgeryConstants.AntiforgeryTokenFieldName,
				tokens.RequestToken,
				new CookieOptions
				{
					HttpOnly = false,
					SameSite = SameSiteMode.Strict,
					Path = this.config.Preview || !string.IsNullOrEmpty(ConfigStorageLocation.VolumeDirectory)
						? "/"
						: $"/{Uri.EscapeDataString(this.config.WebAppName)}"
				}
			);

			return NoContent();
		}

		// POST: api/Authentication/Login
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginCredentials loginCredentials)
		{
			if (this.config.AuthenticationType != AuthenticationType.Cookie)
				return BadRequest();

			var user = await this.userManager.FindByEmailAsync(loginCredentials.Email);
			if (user == null)
				return GetValidationResult(LoginFailedMessage);

			var signInResult = await this.signInManager.PasswordSignInAsync(
				user,
				loginCredentials.Password,
				isPersistent: false,
				lockoutOnFailure: false);

			if (!signInResult.Succeeded)
				return GetValidationResult(LoginFailedMessage);

			return Ok(await this.userService.GetUserClaimsAsync(user));
		}

		// POST: api/Authentication/Logout
		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> Logout()
		{
			var loggedInUser = await this.userManager.GetUserAsync(User);
			if (loggedInUser != null)
			{
				this.sessionFilesJanitor.ClearSession(HttpContext.Session.Id);
				HttpContext.Session.Clear();
				await this.signInManager.SignOutAsync();
			}

			return NoContent();
		}
	}
}
