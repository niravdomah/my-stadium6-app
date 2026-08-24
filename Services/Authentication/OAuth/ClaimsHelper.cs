using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Twenty57.Stadium.WebApp.Spa.Core.Interfaces;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.Authentication.OAuth
{
	internal static class ClaimsHelper
	{
		public static IEnumerable<Claim> BuildClaims(UserInfo userInfo, IUserAdministration userAdministration)
		{
			string isAdministrator = false.ToString();

			if (userAdministration.TryGetUserByUniqueId(userInfo.UniqueId, out var storedUserInfo)
				|| (userInfo.Email != null && userAdministration.TryGetUserByEmail(userInfo.Email, out storedUserInfo)))
			{
				isAdministrator = storedUserInfo.User.IsAdministrator.ToString();
				userInfo.Roles.AddRange(storedUserInfo.Roles);
			}

			var claims = new List<Claim>
			{
				new Claim(CustomClaimTypes.UniqueId, userInfo.UniqueId),
				new Claim(CustomClaimTypes.IsAdministrator, isAdministrator)
			};

			if (userInfo.Email != null)
				claims.Add(new Claim(ClaimTypes.Email, userInfo.Email));

			if (userInfo.Name != null)
				claims.Add(new Claim(ClaimTypes.Name, userInfo.Name));

			var roleClaims = userInfo.Roles.Select(r => new Claim(ClaimTypes.Role, r));
			claims.AddRange(roleClaims);

			return claims;
		}
	}
}
