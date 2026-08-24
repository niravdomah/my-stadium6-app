using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Twenty57.Stadium.WebApp.Spa.Core;
using Twenty57.Stadium.WebApp.Spa.Core.Interfaces;
using Twenty57.Stadium.WebApp.Spa.Template.Areas.Administration.Models.Pages;
using Twenty57.Stadium.WebApp.Spa.Template.Extensions;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Administration.Controllers
{
	[ApiController]
	[Authorize(Policies.AdministratorAccess)]
	[Area("Administration")]
	[Route("api/[area]/[controller]/[action]")]
	public class PagesController : ControllerBase
	{
		private const string auditLogSource = "Web App Users & Roles";

		private readonly IPageAdministration pageAdministration;

		public PagesController(IPageAdministration pageAdministration)
		{
			this.pageAdministration = pageAdministration;
		}

		// GET: api/Administration/Pages/All
		[HttpGet]
		public async Task<IActionResult> All()
		{
			var pageInfos = await this.pageAdministration.GetPagesAsync();
			var pagRecords = pageInfos.Select(p => new PageRecord
			{
				Id = p.Page.Id,
				Name = p.Page.Name,
				IsStartPage = p.Page.IsStartPage,
				MenuItems = p.Page.AccessLookupEntries.Select(a => a.Description).OrderBy(i => i),
				Roles = p.Roles.OrderBy(r => r)
			}).OrderBy(p => p.Name);

			return Ok(pagRecords);
		}

		// PUT: api/Administration/Pages/Edit
		[HttpPut]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(PageToEdit pageModel)
		{
			await this.pageAdministration.EditPageAsync(
				pageModel.Id,
				pageModel.Roles,
				auditLogSource,
				this.GetLoggedInUserId()
			);

			return NoContent();
		}
	}
}
