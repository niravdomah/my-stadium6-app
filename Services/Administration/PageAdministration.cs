using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twenty57.Stadium.WebApp.Spa.Administration;
using Twenty57.Stadium.WebApp.Spa.Core.Entities;
using Twenty57.Stadium.WebApp.Spa.Core.Interfaces;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.Administration
{
	public class PageAdministration : AAdministration, IPageAdministration
	{
		private readonly RoleManager<Role> roleManager;
		private readonly AdministrationContext administrationContext;
		private readonly IAuditLogAdministration auditLogAdministration;

		public PageAdministration(
			RoleManager<Role> roleManager,
			AdministrationContext administrationContext,
			IAuditLogAdministration auditLogAdministration)
		{
			this.roleManager = roleManager;
			this.administrationContext = administrationContext;
			this.auditLogAdministration = auditLogAdministration;
		}

		public async Task<IEnumerable<PageInfo>> GetPagesAsync()
		{
			var pages = await this.administrationContext.Pages
										.Include(p => p.PageRoles)
										.Include(p => p.AccessLookupEntries)
										.ToArrayAsync();

			var roleNamesById = await this.roleManager.Roles.ToDictionaryAsync(r => r.Id, r => r.Name);

			return pages.Select(p => new PageInfo
			{
				Page = p,
				Roles = p.PageRoles.Select(pr => roleNamesById[pr.RoleId])
			});
		}

		public async Task EditPageAsync(string id, IEnumerable<string> roles, string auditLogSource, string auditChangedByUserId)
		{
			var page = await this.administrationContext.Pages
									.Include(p => p.PageRoles)
									.SingleOrDefaultAsync(p => p.Id == id);

			var roleNamesById = await this.roleManager.Roles.ToDictionaryAsync(r => r.Id, r => r.Name);
			string[] oldPageRoles = page.PageRoles
										.Select(pr => pr.RoleId)
										.Select(r => roleNamesById[r])
										.ToArray();

			var roleIdsByName = await this.roleManager.Roles.ToDictionaryAsync(r => r.Name, r => r.Id);

			var newPageRoleIds = roles.Select(r => roleIdsByName[r]).ToHashSet();
			var pageRolesToRemove = page.PageRoles.Where(pr => !newPageRoleIds.Contains(pr.RoleId)).ToArray();
			foreach (var pageRole in pageRolesToRemove)
			{
				page.PageRoles.Remove(pageRole);
			}

			var existingRoleIds = page.PageRoles.Select(pr => pr.RoleId).ToHashSet();
			var pageRoleIdsToAdd = newPageRoleIds.Where(i => !existingRoleIds.Contains(i));
			foreach (string roleId in pageRoleIdsToAdd)
			{
				page.PageRoles.Add(new PageRole
				{
					PageId = page.Id,
					RoleId = roleId
				});
			}

			this.administrationContext.Attach(page);
			this.administrationContext.Update(page);
			await this.administrationContext.SaveChangesAsync();

			await this.auditLogAdministration.LogUpdatePageAsync(
				oldPageRoles,
				page,
				auditLogSource,
				auditChangedByUserId
			);
		}
	}
}
