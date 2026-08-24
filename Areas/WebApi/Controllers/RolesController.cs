using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twenty57.Stadium.WebApp.Spa.Core.Interfaces;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.WebApi.Controllers
{
	[ApiController]
	[Area("WebApi")]
	[Route("api/[controller]")]
	[AllowAnonymous]
	[AuthenticationTypeValidation]
	[UserApiKeyValidation]
	public class RolesController : ControllerBase
	{
		private readonly IRoleAdministration roleAdministration;

		public RolesController(IRoleAdministration roleAdministration)
		{
			this.roleAdministration = roleAdministration;
		}

		// GET: api/roles?key=key
		[HttpGet]
		public async Task<IActionResult> All(string key)
		{
			var roles = new List<string>();

			foreach (var roleInfo in await this.roleAdministration.GetRolesAsync())
			{
				roles.Add(roleInfo.Role.Name);
			}

			return Ok(roles);
		}
	}
}
