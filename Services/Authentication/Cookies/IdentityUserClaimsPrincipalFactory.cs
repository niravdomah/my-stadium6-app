using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;
using Twenty57.Stadium.WebApp.Spa.Core.Entities;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.Authentication.Cookies
{
	public class IdentityUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<Core.Entities.User, Role>
	{
		public IdentityUserClaimsPrincipalFactory(UserManager<Core.Entities.User> userManager, RoleManager<Role> roleManager, IOptions<IdentityOptions> optionsAccessor)
			: base(userManager, roleManager, optionsAccessor)
		{ }

		protected override async Task<ClaimsIdentity> GenerateClaimsAsync(Core.Entities.User user)
		{
			var identity = await base.GenerateClaimsAsync(user);
			identity.AddClaim(
				new Claim(
					CustomClaimTypes.IsAdministrator,
					user.IsAdministrator.ToString(),
					ClaimValueTypes.Boolean
				)
			);

			return identity;
		}
	}
}
