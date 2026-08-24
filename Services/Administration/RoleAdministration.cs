using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twenty57.Stadium.WebApp.Spa.Administration;
using Twenty57.Stadium.WebApp.Spa.Core.Entities;
using Twenty57.Stadium.WebApp.Spa.Core.Interfaces;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.Administration
{
	public class RoleAdministration : AAdministration, IRoleAdministration
	{
		private readonly RoleManager<Role> roleManager;
		private readonly AdministrationContext administrationContext;
		private readonly IAuditLogAdministration auditLogAdministration;

		public RoleAdministration(RoleManager<Role> roleManager, AdministrationContext administrationContext, IAuditLogAdministration auditLogAdministration)
		{
			this.roleManager = roleManager;
			this.administrationContext = administrationContext;
			this.auditLogAdministration = auditLogAdministration;
		}

		public async Task<IEnumerable<RoleInfo>> GetRolesAsync()
		{
			var roles = await this.roleManager.Roles
									.OrderBy(r => r.Name)
									.ToArrayAsync();

			IDictionary<string, string> pageNamesById = await this.administrationContext.Pages
																	.Include(p => p.PageRoles)
																	.ToDictionaryAsync(p => p.Id, p => p.Name);

			return roles.Select(r => new RoleInfo
			{
				Role = r,
				Pages = r.PageRoles.Select(pr => pageNamesById[pr.PageId])
			});
		}

		public async Task AddRoleAsync(string name, IEnumerable<string> pages, string auditLogSource, string auditChangedByUserId)
		{
			var createRoleIdentityResult = await this.roleManager.CreateAsync(new Role(name));
			AssertSuccess(createRoleIdentityResult);

			var newRole = await this.roleManager.FindByNameAsync(name);

			IDictionary<string, string> pageIdsByName = await this.administrationContext.Pages
																	.Include(p => p.PageRoles)
																	.ToDictionaryAsync(p => p.Name, p => p.Id);

			var pageIds = pages.Select(n => pageIdsByName[n]);

			foreach (string pageId in pageIds)
			{
				newRole.PageRoles.Add(new PageRole
				{
					PageId = pageId,
					RoleId = newRole.Id
				});
			}

			await this.administrationContext.SaveChangesAsync();

			await this.auditLogAdministration.LogCreateRoleAsync(newRole, auditLogSource, auditChangedByUserId);
		}

		public async Task EditRoleAsync(string id, string name, IEnumerable<string> pages, string auditLogSource, string auditChangedByUserId)
		{
			var role = await this.roleManager.FindByIdAsync(id);

			var oldRole = new Role(role);
			string[] oldRolePages = await GetRolePageNames(oldRole);

			role.Name = name;

			var updateRoleIdentityResult = await this.roleManager.UpdateAsync(role);
			AssertSuccess(updateRoleIdentityResult);

			IDictionary<string, string> pageIdsByName = await this.administrationContext.Pages
																	.Include(p => p.PageRoles)
																	.ToDictionaryAsync(p => p.Name, p => p.Id);

			var newPageRoleIds = pages.Select(p => pageIdsByName[p]);
			var pageRolesToRemove = role.PageRoles.Where(pr => !newPageRoleIds.Contains(pr.PageId)).ToArray();
			foreach (var pageRole in pageRolesToRemove)
			{
				role.PageRoles.Remove(pageRole);
			}

			var existingPageIds = role.PageRoles.Select(pr => pr.PageId).ToHashSet();
			var pageRoleIdsToAdd = newPageRoleIds.Where(i => !existingPageIds.Contains(i));
			foreach (string pageId in pageRoleIdsToAdd)
			{
				role.PageRoles.Add(new PageRole
				{
					PageId = pageId,
					RoleId = role.Id
				});
			}

			this.administrationContext.Attach(role);
			this.administrationContext.Update(role);
			await this.administrationContext.SaveChangesAsync();

			await this.auditLogAdministration.LogUpdateRoleAsync(
				oldRole,
				oldRolePages,
				role,
				auditLogSource,
				auditChangedByUserId
			);
		}

		public async Task DeleteRoleAsync(string id, string auditLogSource, string auditChangedByUserId)
		{
			var role = await this.roleManager.FindByIdAsync(id);

			var oldRole = new Role(role);
			string[] oldRolePages = await GetRolePageNames(oldRole);

			var deleteRoleIdentityResult = await this.roleManager.DeleteAsync(role);
			AssertSuccess(deleteRoleIdentityResult);

			await this.auditLogAdministration.LogDeleteRoleAsync(
				oldRole,
				oldRolePages,
				auditLogSource,
				auditChangedByUserId
			);
		}

		private async Task<string[]> GetRolePageNames(Role role)
		{
			var pageNamesById = await this.administrationContext.Pages
											.Include(p => p.PageRoles)
											.ToDictionaryAsync(p => p.Id, p => p.Name);

			return role.PageRoles.Select(pr => pageNamesById[pr.PageId]).ToArray();
		}
	}
}
