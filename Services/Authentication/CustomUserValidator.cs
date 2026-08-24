using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Twenty57.Stadium.WebApp.Spa.Core.Enums;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Configuration;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.Authentication
{
	public class CustomUserValidator<TUser> : UserValidator<TUser>
		where TUser : Core.Entities.User
	{
		public override Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
		{
			var authenticationType = AppSettingsHelper.GetInstance().AppSettings.Config.AuthenticationType;

			if (authenticationType == AuthenticationType.OAuth && string.IsNullOrEmpty(user.Email))
			{
				return Task.FromResult(IdentityResult.Success);
			}

			return base.ValidateAsync(manager, user);
		}
	}
}
