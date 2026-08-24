using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Twenty57.Stadium.WebApp.Spa.Template.Services.Authentication;

namespace Twenty57.Stadium.WebApp.Spa.Template.Extensions
{
	public static class ControllerBaseExtensions
	{
		public static string GetLoggedInUserId(this ControllerBase controllerBase)
		{
			return controllerBase.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? controllerBase.User.FindFirstValue(CustomClaimTypes.UniqueId);
		}
	}
}
