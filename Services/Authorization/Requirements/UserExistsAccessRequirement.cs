using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Twenty57.Stadium.WebApp.Spa.Core.Enums;
using Twenty57.Stadium.WebApp.Spa.Template.Services.Authentication;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.Authorization.Requirements
{
	public class UserExistsAccessRequirement : IAuthorizationRequirement
	{ }

	public class UserExistsAccessHandler : AuthorizationHandler<UserExistsAccessRequirement>
	{
		private readonly UserManager<Core.Entities.User> userManager;

		public UserExistsAccessHandler(UserManager<Core.Entities.User> userManager)
		{
			this.userManager = userManager;
		}

		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserExistsAccessRequirement requirement)
		{
			if (context.User?.Identity == null)
			{
				context.Fail();
				return Task.CompletedTask;
			}

			if (context.User.Identity.AuthenticationType == AuthenticationSchemes.GetDefaultAuthenticationScheme(AuthenticationType.Cookie))
			{
				var loggedInUser = this.userManager.GetUserAsync(context.User).Result;
				if (loggedInUser == null)
				{
					context.Fail();
					return Task.CompletedTask;
				}
			}

			context.Succeed(requirement);
			return Task.CompletedTask;
		}
	}
}
