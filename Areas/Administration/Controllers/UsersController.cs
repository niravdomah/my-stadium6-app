using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Twenty57.Stadium.WebApp.Spa.Core;
using Twenty57.Stadium.WebApp.Spa.Core.Entities;
using Twenty57.Stadium.WebApp.Spa.Core.Interfaces;
using Twenty57.Stadium.WebApp.Spa.Template.Areas.Administration.Models.Users;
using Twenty57.Stadium.WebApp.Spa.Template.Extensions;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Administration.Controllers
{
	[ApiController]
	[Authorize(Policies.AdministratorAccess)]
	[Area("Administration")]
	[Route("api/[area]/[controller]/[action]")]
	public class UsersController : ControllerBase
	{
		private const string auditLogSource = "Web App Users & Roles";

		private readonly IUserAdministration userAdministration;
		private readonly SignInManager<User> signInManager;

		public UsersController(IUserAdministration userAdministration, SignInManager<User> signInManager)
		{
			this.userAdministration = userAdministration;
			this.signInManager = signInManager;
		}

		// GET: api/Administration/Users/All
		[HttpGet]
		public async Task<IActionResult> All()
		{
			var userInfos = await this.userAdministration.GetUsersAsync();
			int administratorCount = userInfos.Count(u => u.User.IsAdministrator);

			string loggedInUserId = this.GetLoggedInUserId();

			var userRecords = userInfos.Select(u => new UserRecord
			{
				Id = u.User.Id,
				Email = u.User.Email,
				UserName = u.User.UserName,
				Name = u.User.Name,
				IsAdministrator = u.User.IsAdministrator,
				IsLastAdministrator = u.User.IsAdministrator && administratorCount <= 1,
				IsLoggedIn = u.User.Id == loggedInUserId,
				Roles = u.Roles
			}).OrderBy(u => u.UserName);

			return Ok(userRecords);
		}

		// POST: api/Administration/Users/Add
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Add(UserToAdd userModel)
		{
			await this.userAdministration.AddUserAsync(
				email: userModel.Email,
				name: userModel.Name,
				userName: userModel.UserName,
				password: userModel.Password,
				isAdministrator: userModel.IsAdministrator,
				roles: userModel.Roles,
				auditLogSource: auditLogSource,
				auditChangedByUserId: this.GetLoggedInUserId()
			);

			return NoContent();
		}

		// PUT: api/Administration/Users/Edit
		[HttpPut]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(UserToEdit userModel)
		{
			await this.userAdministration.EditUserAsync(
				id: userModel.Id,
				email: userModel.Email,
				name: userModel.Name ?? string.Empty,
				userName: userModel.UserName,
				isAdministrator: userModel.Id == this.GetLoggedInUserId() ? null : userModel.IsAdministrator,
				password: userModel.Password,
				roles: userModel.Roles,
				auditLogSource: auditLogSource,
				auditChangedByUserId: this.GetLoggedInUserId()
			);

			var userInfo = await this.userAdministration.GetUserByIdAsync(userModel.Id);

			if (userModel.Id == this.GetLoggedInUserId())
				await this.signInManager.RefreshSignInAsync(userInfo.User);

			return NoContent();
		}

		// PUT: api/Administration/Users/EditIsAdministrator
		[HttpPut]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditIsAdministrator(IsAdministratorToEdit userModel)
		{
			await this.userAdministration.EditIsAdministrator(
				id: userModel.Id,
				isAdministrator: userModel.IsAdministrator,
				auditLogSource: auditLogSource,
				auditChangedByUserId: this.GetLoggedInUserId()
			);

			return NoContent();
		}

		// DELETE: api/Administration/Users/Delete?id=[id]
		[HttpDelete]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete([FromQuery] UserToDelete userModel)
		{
			await this.userAdministration.DeleteUserAsync(
				userModel.Id,
				auditLogSource,
				this.GetLoggedInUserId()
			);

			return NoContent();
		}
	}
}
