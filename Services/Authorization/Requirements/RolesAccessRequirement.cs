using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Twenty57.Stadium.WebApp.Spa.Administration;
using Twenty57.Stadium.WebApp.Spa.Core.Enums;
using Twenty57.Stadium.WebApp.Spa.Template.Areas.Documents;
using Twenty57.Stadium.WebApp.Spa.Template.Services.Authentication;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.Authorization.Requirements
{
	public class RolesAccessRequirement : IAuthorizationRequirement
	{ }

	public class RolesAccessHandler : AuthorizationHandler<RolesAccessRequirement>
	{
		private readonly AdministrationContext administrationContext;

		public RolesAccessHandler(AdministrationContext administrationContext)
		{
			this.administrationContext = administrationContext;
		}

		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RolesAccessRequirement requirement)
		{
			if (context.User?.Identity == null)
			{
				context.Fail();
				return Task.CompletedTask;
			}

			if (context.User.Identity.AuthenticationType == AuthenticationSchemes.GetDefaultAuthenticationScheme(AuthenticationType.Anonymous))
			{
				context.Succeed(requirement);
				return Task.CompletedTask;
			}

			IEnumerable<string> roles;
			if (TryGetRolesFromPageUseAttribute(context, out var pageUseRoles))
			{
				roles = pageUseRoles;
			}
			else if (TryGetPageRoles(context, out var pageRoles))
			{
				roles = pageRoles;
			}
			else
			{
				context.Fail();
				return Task.CompletedTask;
			}

			bool userIsInRoles = roles.Any(r => context.User.IsInRole(r));
			if (userIsInRoles)
			{
				context.Succeed(requirement);
				return Task.CompletedTask;
			}
			else
			{
				// set response message to "You don't have any roles that can view this page. If you need to get access to this page, please contact your administrator."
				// handle message to display ClientSide or in WebApi?
			}

			context.Fail();
			return Task.CompletedTask;
		}

		private bool TryGetRolesFromPageUseAttribute(AuthorizationHandlerContext context, out IEnumerable<string> pageUseRoles)
		{
			var defaultHttpContext = context.Resource as DefaultHttpContext;
			var controllerActionDescriptor = defaultHttpContext.GetEndpoint()
																.Metadata
																.GetMetadata<ControllerActionDescriptor>();

			var pageUseAttribute = controllerActionDescriptor.ControllerTypeInfo.GetCustomAttribute<PageUseAttribute>();
			if (pageUseAttribute == null)
			{
				pageUseRoles = null;
				return false;
			}

			pageUseRoles = pageUseAttribute.Pages.SelectMany(p => GetPageRolesAsync(p, this.administrationContext).Result).Distinct();
			return true;
		}

		private bool TryGetPageRoles(AuthorizationHandlerContext context, out IEnumerable<string> pageRoles)
		{
			var defaultHttpContext = context.Resource as DefaultHttpContext;
			var controllerActionDescriptor = defaultHttpContext.GetEndpoint()
																.Metadata
																.GetMetadata<ControllerActionDescriptor>();

			string pageName = controllerActionDescriptor.ControllerName;
			if (pageName == null)
			{
				pageRoles = null;
				return false;
			}

			pageRoles = GetPageRolesAsync(pageName, this.administrationContext).Result;
			return true;
		}

		private async Task<IEnumerable<string>> GetPageRolesAsync(string pageName, AdministrationContext administrationContext)
		{
			var pageEntity = await administrationContext.Pages
										.Include(p => p.PageRoles)
										.SingleOrDefaultAsync(p => p.Name == pageName);

			if (pageEntity == null)
				throw new Exception($"Page {pageName} does not exist.");

			var rolesIds = pageEntity.PageRoles.Select(pr => pr.RoleId).ToHashSet();
			var roles = administrationContext.Roles
							.Where(r => rolesIds.Contains(r.Id))
							.Select(r => r.Name)
							.AsEnumerable();

			return roles;
		}
	}
}
