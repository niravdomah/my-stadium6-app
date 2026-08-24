using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twenty57.Stadium.Core.Helpers;
using Twenty57.Stadium.WebApp.Spa.Core.Entities;
using Twenty57.Stadium.WebApp.Spa.Template.Areas.Authentication.Models.Password;
using Twenty57.Stadium.WebApp.Spa.Template.Controllers;
using Twenty57.Stadium.WebApp.Spa.Template.Services.PasswordReset;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Authentication.Controllers
{
	[ApiController]
	[Area("Authentication")]
	[Route("api/[area]/[controller]/[action]")]
	public class PasswordController : AValidatedControllerBase
	{
		private readonly UserManager<User> userManager;
		private readonly PasswordResetService passwordResetService;

		public PasswordController(
			UserManager<User> userManager,
			PasswordResetService passwordResetService)
		{
			this.userManager = userManager;
			this.passwordResetService = passwordResetService;
		}

		// POST: api/Authentication/Password/ChangePassword
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ChangePassword(ChangePasswordInfo changePasswordInfo)
		{
			var user = await this.userManager.GetUserAsync(User);

			var changePasswordIdentityResult = await this.userManager.ChangePasswordAsync(user, changePasswordInfo.OldPassword, changePasswordInfo.NewPassword);
			if (!changePasswordIdentityResult.Succeeded)
				return GetValidationResult(string.Join(", ", changePasswordIdentityResult.Errors.Select(e => e.Description)));

			return NoContent();
		}

		// GET: api/Authentication/Password/CanResetPassword
		[HttpGet]
		[AllowAnonymous]
		public IActionResult CanResetPassword()
		{
			try
			{
				var smtpSettings = this.passwordResetService.GetSmtpSettings();

				if (smtpSettings == null)
					return GetValidationResult("No SMTP settings found. Please contact the Administrator for this application.");

				if (!this.passwordResetService.AreSmtpSettingsValid(smtpSettings))
					return GetValidationResult("No valid SMTP settings found. Please contact the Administrator for this application.");

				return Ok(true);
			}
			catch (Exception)
			{
				return GetValidationResult("Could not reset password. Please contact the Administrator for this application.");
			}
		}

		// POST: api/Authentication/Password/SendPasswordResetEmail
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> SendPasswordResetEmail(PasswordResetInfo passwordResetInfo)
		{
			var user = await this.userManager.FindByEmailAsync(passwordResetInfo.Email);
			if (user == null)
				return GetValidationResult("Email not found");

			string passwordResetToken = await this.userManager.GeneratePasswordResetTokenAsync(user);

			string resetPasswordUrl = UrlHelper.AddQueryString(
				passwordResetInfo.PasswordResetUrl,
				new Dictionary<string, string>
				{
					{ "userId", user.Id },
					{ "passwordResetToken", passwordResetToken }
				});

			this.passwordResetService.SendPasswordResetEmail(passwordResetInfo.Email, resetPasswordUrl);

			return Ok();
		}

		// POST: api/Authentication/Password/UpdatePassword
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdatePassword(UpdatePasswordInfo updatePasswordInfo)
		{
			var user = await this.userManager.FindByIdAsync(updatePasswordInfo.UserId);
			if (user == null)
				return GetValidationResult("User not found");

			var resetPasswordIdentityResult = await this.userManager.ResetPasswordAsync(user, updatePasswordInfo.PasswordResetToken, updatePasswordInfo.NewPassword);
			if (!resetPasswordIdentityResult.Succeeded)
				return GetValidationResult(string.Join(", ", resetPasswordIdentityResult.Errors.Select(e => e.Description)));

			return NoContent();
		}
	}
}
