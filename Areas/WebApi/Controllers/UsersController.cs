using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Twenty57.Stadium.WebApp.Spa.Core.Interfaces;
using Twenty57.Stadium.WebApp.Spa.Template.Areas.WebApi.Models;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.WebApi.Controllers
{
	[ApiController]
	[Area("WebApi")]
	[Route("api/[controller]")]
	[AllowAnonymous]
	[AuthenticationTypeValidation]
	[UserApiKeyValidation]
	public class UsersController : ControllerBase
	{
		private const string auditLogSource = "User API";

		private readonly IUserAdministration userAdministration;

		public UsersController(IUserAdministration userAdministration)
		{
			this.userAdministration = userAdministration;
		}

		// GET: api/users?key=key&email=email
		[HttpGet]
		public async Task<IActionResult> All(string key, string email = null)
		{
			var users = await FilterUsers(email);

			return Ok(users);
		}

		// GET: api/users/count?key=key
		[HttpGet]
		[Route("count")]
		public async Task<IActionResult> Count(string key)
		{
			int count = (await this.userAdministration.GetUsersAsync()).Count();

			return Ok(count);
		}

		// GET: api/users/<id>?key=key
		[HttpGet]
		[Route("{id}")]
		public async Task<IActionResult> GetUserById(string id, string key)
		{
			var userInfo = await this.userAdministration.GetUserByIdAsync(id);

			return Ok(new User
			{
				Id = userInfo.User.Id,
				Email = userInfo.User.Email,
				Name = userInfo.User.Name,
				Username = userInfo.User.UserName,
				IsAdministrator = userInfo.User.IsAdministrator,
				Roles = userInfo.Roles.ToArray(),
				Data = userInfo.User.UniqueId == null ? new object() : new
				{
					externalId = userInfo.User.UniqueId
				}
			});
		}

		// GET: api/users/find?key=key&email=email
		[HttpGet]
		[Route("find")]
		public async Task<IActionResult> GetUserByEmail(string key, string email)
		{
			if (string.IsNullOrEmpty(email))
			{
				return Ok(Array.Empty<User>());
			}

			var users = await FilterUsers(email);

			return Ok(users);
		}

		// POST: api/users?key=key
		[HttpPost]
		public async Task<IActionResult> Add(string key, [FromBody] UserInfo userToAdd)
		{
			await this.userAdministration.AddUserAsync(
				email: userToAdd.Email,
				name: userToAdd.Name,
				userName: userToAdd.Username,
				password: userToAdd.Password,
				isAdministrator: userToAdd.IsAdministrator ?? false,
				roles: userToAdd.Roles,
				auditLogSource: auditLogSource,
				auditChangedByUserId: null
			);

			var userInfo = await this.userAdministration.GetUserByEmailAsync(userToAdd.Email);

			return Ok(new
			{
				userID = userInfo.User.Id
			});
		}

		// PUT: api/users/<id>?key=key
		[HttpPut]
		[Route("{id}")]
		public async Task<IActionResult> Edit(string id, string key, [FromBody] UserInfo userModel)
		{
			await this.userAdministration.EditUserAsync(
				id: id,
				email: userModel.Email,
				name: userModel.Name,
				userName: userModel.Username,
				isAdministrator: userModel.IsAdministrator ?? false,
				password: userModel.Password,
				roles: userModel.Roles,
				auditLogSource: auditLogSource,
				auditChangedByUserId: null
			);

			return Ok(new
			{
				Message = "User updated successfully"
			});
		}

		// DELETE: api/users/<id>?key=key
		[HttpDelete]
		[Route("{id}")]
		public async Task<IActionResult> Delete(string id, string key)
		{
			await this.userAdministration.DeleteUserAsync(
				id: id,
				auditLogSource: auditLogSource,
				auditChangedByUserId: null
			);

			return Ok(new
			{
				Message = "User deleted successfully"
			});
		}

		private async Task<User[]> FilterUsers(string emailPart = null)
		{
			var userInfos = await this.userAdministration.GetUsersAsync();

			return userInfos
				.Where(u =>
				{
					if (string.IsNullOrEmpty(emailPart))
						return true;

					return u.User.NormalizedEmail.Contains(emailPart, StringComparison.InvariantCultureIgnoreCase);
				})
				.Select(u => new User
				{
					Id = u.User.Id,
					Email = u.User.Email,
					Name = u.User.Name,
					Username = u.User.UserName,
					IsAdministrator = u.User.IsAdministrator,
					Roles = u.Roles.ToArray(),
					Data = u.User.UniqueId == null ? new object() : new
					{
						externalId = u.User.UniqueId
					}
				})
				.OrderBy(u => string.IsNullOrEmpty(u.Email) ? u.Username : u.Email)
				.ToArray();
		}
	}
}
