using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Twenty57.Stadium.WebApp.Spa.Template.Services.Authentication;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.Authorization.Requirements
{
	public class AdministratorAccessRequirement : IAuthorizationRequirement
	{ }

	public class AdministratorAccessHandler : AuthorizationHandler<AdministratorAccessRequirement>
	{
		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdministratorAccessRequirement requirement)
		{
			if (context.User?.Identity == null)
			{
				context.Fail();
				return Task.CompletedTask;
			}

			if (context.User.HasClaim(CustomClaimTypes.IsAdministrator, true.ToString()))
			{
				context.Succeed(requirement);
				return Task.CompletedTask;
			}

			context.Fail();
			return Task.CompletedTask;
		}
	}
}
