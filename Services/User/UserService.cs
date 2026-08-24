using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Twenty57.Stadium.WebApp.Spa.Administration;
using Twenty57.Stadium.WebApp.Spa.Administration.Interfaces;
using Twenty57.Stadium.WebApp.Spa.Core.Entities;
using Twenty57.Stadium.WebApp.Spa.Core.Enums;
using Twenty57.Stadium.WebApp.Spa.Core.Interfaces;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Configuration;
using Twenty57.Stadium.WebApp.Spa.Template.Services.Authentication;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.User
{
	public class UserService : IUserService
	{
		private readonly Config config;
		private readonly AdministrationContext administrationContext;
		private readonly IUserStore userStore;
		private readonly UserManager<Core.Entities.User> userManager;
		private readonly IHttpContextAccessor httpContextAccessor;
		private readonly ILookupNormalizer keyNormalizer;

		public UserService(
			IOptionsSnapshot<Config> configOptions,
			AdministrationContext administrationContext,
			IUserStore userStore,
			UserManager<Core.Entities.User> userManager,
			IHttpContextAccessor httpContextAccessor,
			ILookupNormalizer keyNormalizer)
		{
			this.config = configOptions.Value;
			this.administrationContext = administrationContext;
			this.userStore = userStore;
			this.userManager = userManager;
			this.httpContextAccessor = httpContextAccessor;
			this.keyNormalizer = keyNormalizer;
		}

		public UserClaims GetLoggedInUserClaims()
		{
			return GetLoggedInUserClaimsAsync().Result;
		}

		public async Task<UserClaims> GetLoggedInUserClaimsAsync()
		{
			var loggedInUser = await GetLoggedInUserAsync();
			if (loggedInUser == null)
				return null;

			var externalRoles = this.config.AuthenticationType == AuthenticationType.OAuth
				? this.httpContextAccessor.HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value)
				: null;

			return await GetUserClaimsAsync(loggedInUser, externalRoles);
		}

		public async Task<UserClaims> GetUserClaimsAsync(Core.Entities.User user, IEnumerable<string> externalRoles = null)
		{
			var roles = (await this.userManager.GetRolesAsync(user)).AsEnumerable();
			if (externalRoles != null)
				roles = roles.Concat(externalRoles).Distinct();

			return new UserClaims
			{
				Id = user.Id,
				Email = user.Email,
				Username = user.UserName,
				Name = user.Name,
				IsAdministrator = user.IsAdministrator,
				AccessiblePages = GetAccessiblePages(user, externalRoles),
				Roles = roles.OrderBy(r => r).ToArray()
			};
		}

		private async Task<Core.Entities.User> GetLoggedInUserAsync()
		{
			var loggedInUserClaimsPrincipal = this.httpContextAccessor.HttpContext.User;
			if (loggedInUserClaimsPrincipal == null)
				return null;

			if (this.config.AuthenticationType == AuthenticationType.Cookie)
				return await this.userManager.GetUserAsync(loggedInUserClaimsPrincipal);

			if (this.config.AuthenticationType == AuthenticationType.OAuth)
			{
				string uniqueId = loggedInUserClaimsPrincipal.Claims.SingleOrDefault(c => c.Type == CustomClaimTypes.UniqueId)?.Value;
				if (uniqueId != null)
					return await this.userStore.FindByUniqueIdAsync(uniqueId);
			}

			if (this.config.AuthenticationType == AuthenticationType.Windows)
			{
				string name = loggedInUserClaimsPrincipal.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
				if (!string.IsNullOrEmpty(name))
				{
					var user = await this.userStore.FindByNameAsync(this.keyNormalizer.NormalizeName(name), default);
					return user;
				}
			}

			return null;
		}

		private string[] GetAccessiblePages(Core.Entities.User user, IEnumerable<string> externalRoles)
		{
			var roleNamesById = this.administrationContext.Roles.ToDictionary(r => r.Id, r => r.Name);

			var userRoleNames = user.UserRoles
									.Select(ur => roleNamesById[ur.RoleId])
									.ToHashSet();

			if (externalRoles != null)
				userRoleNames.UnionWith(externalRoles);

			var pages = this.administrationContext.Pages.Include(p => p.PageRoles);

			var accessiblePages = new List<string>();
			foreach (var page in pages)
			{
				if (page.PageRoles.Any(pr => userRoleNames.Contains(roleNamesById[pr.RoleId])))
				{
					accessiblePages.Add(page.Name);
				}
			}

			return accessiblePages.ToArray();
		}
	}
}
