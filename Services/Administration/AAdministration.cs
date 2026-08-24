using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.Administration
{
	public abstract class AAdministration
	{
		protected void AssertSuccess(IdentityResult identityResult)
		{
			if (!identityResult.Succeeded)
			{
				string errorMessage = string.Join(", ", identityResult.Errors.Select(e => e.Description));
				throw new Exception(errorMessage);
			}
		}
	}
}
