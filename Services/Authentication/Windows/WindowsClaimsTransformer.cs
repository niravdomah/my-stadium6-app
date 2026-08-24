using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Twenty57.Stadium.WebApp.Spa.Core.Enums;
using Twenty57.Stadium.WebApp.Spa.Core.Interfaces;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Configuration;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.Authentication.Windows
{
	public class WindowsClaimsTransformer : IClaimsTransformation
	{
		private readonly IUserAdministration userAdministration;

		public WindowsClaimsTransformer(IUserAdministration userAdministration)
		{
			this.userAdministration = userAdministration;
		}

		public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
		{
			var authenticationType = AppSettingsHelper.GetInstance().AppSettings.Config.AuthenticationType;
			if (authenticationType != AuthenticationType.Windows)
				return Task.FromResult(principal);

			var identity = (ClaimsIdentity)principal.Identity;

			if (this.userAdministration.TryGetUserByName(identity.Name, out var userInfo))
			{
				var claims = new List<Claim>
				{
					new Claim(CustomClaimTypes.UniqueId, userInfo.User.Id),
					new Claim(CustomClaimTypes.IsAdministrator, userInfo.User.IsAdministrator.ToString())
				};

				if (userInfo.User.Email != null)
					claims.Add(new Claim(ClaimTypes.Email, userInfo.User.Email));

				if (userInfo.User.Name != null)
					claims.Add(new Claim(ClaimTypes.Name, userInfo.User.Name));

				var roleClaims = userInfo.Roles.Select(r => new Claim(identity.RoleClaimType, r));
				claims.AddRange(roleClaims);

				identity.AddClaims(claims);
			}

			return Task.FromResult(principal);
		}
	}
}
