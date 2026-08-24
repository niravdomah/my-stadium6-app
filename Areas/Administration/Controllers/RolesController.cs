using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Twenty57.Stadium.WebApp.Spa.Core;
using Twenty57.Stadium.WebApp.Spa.Core.Interfaces;
using Twenty57.Stadium.WebApp.Spa.Template.Areas.Administration.Models.Roles;
using Twenty57.Stadium.WebApp.Spa.Template.Extensions;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Administration.Controllers
{
	[ApiController]
	[Authorize(Policies.AdministratorAccess)]
	[Area("Administration")]
	[Route("api/[area]/[controller]/[action]")]
	public class RolesController : ControllerBase
	{
		private const string auditLogSource = "Web App Users & Roles";

		private readonly IRoleAdministration roleAdministration;

		public RolesController(IRoleAdministration roleAdministration)
		{
			this.roleAdministration = roleAdministration;
		}

		// GET: api/Administration/Roles/All
		[HttpGet]
		public async Task<IActionResult> All()
		{
			var roleInfos = await this.roleAdministration.GetRolesAsync();

			var roleModels = roleInfos.Select(r => new RoleRecord
			{
				Id = r.Role.Id,
				Name = r.Role.Name,
				IsDefault = r.Role.IsDefault,
				Pages = r.Pages.OrderBy(p => p)
			}).OrderBy(r => r.Name);

			return Ok(roleModels);
		}

		// POST: api/Administration/Roles/Add
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Add(RoleBase roleModel)
		{
			await this.roleAdministration.AddRoleAsync(
				roleModel.Name,
				roleModel.Pages,
				auditLogSource,
				this.GetLoggedInUserId()
			);

			return NoContent();
		}

		// PUT: api/Administration/Roles/Edit
		[HttpPut]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(RoleToEdit roleModel)
		{
			await this.roleAdministration.EditRoleAsync(
				roleModel.Id,
				roleModel.Name,
				roleModel.Pages,
				auditLogSource,
				this.GetLoggedInUserId()
			);

			return NoContent();
		}

		// DELETE: api/Administration/Roles/Delete?id=[id]
		[HttpDelete]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete([FromQuery] RoleToDelete roleModel)
		{
			await this.roleAdministration.DeleteRoleAsync(
				roleModel.Id,
				auditLogSource,
				this.GetLoggedInUserId()
			);

			return NoContent();
		}
	}
}
