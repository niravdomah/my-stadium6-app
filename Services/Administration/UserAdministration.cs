using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twenty57.Stadium.WebApp.Spa.Core.Entities;
using Twenty57.Stadium.WebApp.Spa.Core.Enums;
using Twenty57.Stadium.WebApp.Spa.Core.Interfaces;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Configuration;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.Administration
{
	public class UserAdministration : AAdministration, IUserAdministration
	{
		private readonly UserManager<Core.Entities.User> userManager;
		private readonly RoleManager<Role> roleManager;
		private readonly IAuditLogAdministration auditLogAdministration;

		public UserAdministration(
			UserManager<Core.Entities.User> userManager,
			RoleManager<Role> roleManager,
			IAuditLogAdministration auditLogAdministration)
		{
			this.userManager = userManager;
			this.roleManager = roleManager;
			this.auditLogAdministration = auditLogAdministration;
		}

		public async Task<bool> EmailExistsAsync(string email)
		{
			var user = await this.userManager.FindByEmailAsync(email);
			return user != null;
		}

		public async Task<IEnumerable<UserInfo>> GetUsersAsync()
		{
			var users = await this.userManager.Users.ToArrayAsync();
			var roleNamesById = await this.roleManager.Roles.ToDictionaryAsync(r => r.Id, r => r.Name);

			return users.Select(u => new UserInfo
			{
				User = u,
				Roles = u.UserRoles.Select(ur => roleNamesById[ur.RoleId])
			});
		}

		public async Task<UserInfo> GetUserByIdAsync(string id)
		{
			var user = await this.userManager.FindByIdAsync(id);
			if (user == null)
				throw new Exception("User not found");

			string[] roles = (await this.userManager.GetRolesAsync(user)).ToArray();

			return new UserInfo
			{
				User = user,
				Roles = roles
			};
		}

		public async Task<UserInfo> GetUserByEmailAsync(string email)
		{
			var user = await this.userManager.FindByEmailAsync(email);
			if (user == null)
				throw new Exception("User not found");

			string[] roles = (await this.userManager.GetRolesAsync(user)).ToArray();

			return new UserInfo
			{
				User = user,
				Roles = roles
			};
		}

		public async Task<UserInfo> GetUserByNameAsync(string userName)
		{
			var user = await this.userManager.FindByNameAsync(userName);
			if (user == null)
				throw new Exception("User not found");

			string[] roles = (await this.userManager.GetRolesAsync(user)).ToArray();

			return new UserInfo
			{
				User = user,
				Roles = roles
			};
		}

		public bool TryGetUserByEmail(string email, out UserInfo userInfo)
		{
			if (email == null)
				throw new ArgumentNullException(nameof(email));

			var user = this.userManager.FindByEmailAsync(email).Result;
			if (user == null)
			{
				userInfo = null;
				return false;
			}

			string[] roles = this.userManager.GetRolesAsync(user).Result.ToArray();
			userInfo = new UserInfo
			{
				User = user,
				Roles = roles
			};

			return true;
		}

		public bool TryGetUserByName(string userName, out UserInfo userInfo)
		{
			if (userName == null)
				throw new ArgumentNullException(nameof(userName));

			var user = this.userManager.FindByNameAsync(userName).Result;
			if (user == null)
			{
				userInfo = null;
				return false;
			}

			string[] roles = this.userManager.GetRolesAsync(user).Result.ToArray();
			userInfo = new UserInfo
			{
				User = user,
				Roles = roles
			};

			return true;
		}

		public bool TryGetUserByUniqueId(string uniqueId, out UserInfo userInfo)
		{
			if (uniqueId == null)
				throw new ArgumentNullException(nameof(uniqueId));

			var user = this.userManager.Users.SingleOrDefault(u => u.UniqueId == uniqueId);
			if (user == null)
			{
				userInfo = null;
				return false;
			}

			string[] roles = this.userManager.GetRolesAsync(user).Result.ToArray();
			userInfo = new UserInfo
			{
				User = user,
				Roles = roles
			};

			return true;
		}

		public async Task AddUserAsync(
			string email,
			string name,
			string userName,
			string password,
			bool isAdministrator,
			IEnumerable<string> roles,
			string uniqueId = null,
			string auditLogSource = null,
			string auditChangedByUserId = null)
		{
			ArgumentNullException.ThrowIfNull(auditLogSource);

			var authenticationType = AppSettingsHelper.GetInstance().AppSettings.Config.AuthenticationType;

			if (authenticationType == AuthenticationType.OAuth && string.IsNullOrEmpty(email))
			{
				CheckRequiredField("UniqueId", uniqueId);
				CheckUserDoesNotExistByUniqueId(uniqueId);
			}
			else
			{
				CheckRequiredField("Email", email);
				CheckUserDoesNotExistByEmail(email);
			}

			if (authenticationType == AuthenticationType.Cookie)
				CheckRequiredField("Password", password);

			if (authenticationType == AuthenticationType.Windows)
				CheckRequiredField("UserName", userName);

			var newUser = new Core.Entities.User(userName: userName ?? email ?? uniqueId)
			{
				Email = email,
				Name = name,
				IsAdministrator = isAdministrator,
				UniqueId = uniqueId
			};

			var createUserIdentityResult = password == null
				? await this.userManager.CreateAsync(newUser)
				: await this.userManager.CreateAsync(newUser, password);
			AssertSuccess(createUserIdentityResult);

			var rolesToAdd = roles?.ToHashSet() ?? [];

			if (roles == null || !roles.Any(r => r == Role.DefaultRoleName))
				rolesToAdd = rolesToAdd?.Append(Role.DefaultRoleName).ToHashSet();

			var addToRoleIdentityResult = await this.userManager.AddToRolesAsync(newUser, rolesToAdd);
			AssertSuccess(addToRoleIdentityResult);

			await this.auditLogAdministration.LogCreateUserAsync(
				newUser,
				auditLogSource,
				auditChangedByUserId
			);
		}

		public async Task EditUserAsync(
			string id,
			string email = null,
			string name = null,
			string userName = null,
			bool? isAdministrator = null,
			string password = null,
			IEnumerable<string> roles = null,
			string uniqueId = null,
			string auditLogSource = null,
			string auditChangedByUserId = null)
		{
			CheckRequiredField("User ID", id);

			var user = await this.userManager.FindByIdAsync(id);
			if (user == null)
				throw new Exception("User not found");

			ArgumentNullException.ThrowIfNull(auditLogSource);

			var oldUser = new Core.Entities.User(user);
			string[] oldRoles = (await this.userManager.GetRolesAsync(user)).ToArray();

			var authenticationType = AppSettingsHelper.GetInstance().AppSettings.Config.AuthenticationType;
			if (authenticationType == AuthenticationType.Windows)
			{
				CheckRequiredField("UserName", userName);

				user.UserName = userName;
			}

			if (email != null)
			{
				CheckUserDoesNotExistByEmail(email, id);

				user.Email = email;

				if (authenticationType != AuthenticationType.Windows)
					user.UserName = email;
			}

			if (name == string.Empty)
			{
				user.Name = null;
			}
			else if (name != null)
			{
				user.Name = name;
			}

			if (isAdministrator.HasValue)
			{
				if (!isAdministrator.Value)
					CheckIsNotLastAdmin(id);

				user.IsAdministrator = isAdministrator.Value;
			}

			if (!string.IsNullOrWhiteSpace(password))
				user.PasswordHash = this.userManager.PasswordHasher.HashPassword(user, password);

			if (uniqueId != null)
			{
				user.UniqueId = uniqueId;

				if (authenticationType != AuthenticationType.Windows)
					user.UserName = uniqueId;
			}

			var updateUserIdentityResult = await this.userManager.UpdateAsync(user);
			AssertSuccess(updateUserIdentityResult);

			if (roles != null)
			{
				var existingRolesSet = (await this.userManager.GetRolesAsync(user)).ToHashSet();
				var newRolesSet = roles.ToHashSet();

				var rolesToRemove = new List<string>();
				foreach (string role in existingRolesSet)
				{
					if (role != Role.DefaultRoleName && !newRolesSet.Contains(role))
						rolesToRemove.Add(role);
				}
				var removeRolesIdentityResult = await this.userManager.RemoveFromRolesAsync(user, rolesToRemove);
				AssertSuccess(removeRolesIdentityResult);

				var rolesToAdd = new List<string>();
				foreach (string role in newRolesSet)
				{
					if (!existingRolesSet.Contains(role))
						rolesToAdd.Add(role);
				}

				if (!rolesToAdd.Any(r => r == Role.DefaultRoleName) && !existingRolesSet.Any(r => r == Role.DefaultRoleName))
					rolesToAdd.Add(Role.DefaultRoleName);

				var addToRolesIdentityResult = await this.userManager.AddToRolesAsync(user, rolesToAdd);
				AssertSuccess(addToRolesIdentityResult);
			}

			await this.auditLogAdministration.LogUpdateUserAsync(
				oldUser,
				oldRoles,
				user,
				auditLogSource,
				auditChangedByUserId
			);
		}

		public async Task EditIsAdministrator(string id, bool isAdministrator, string auditLogSource, string auditChangedByUserId)
		{
			var user = await this.userManager.FindByIdAsync(id);
			if (user == null)
				throw new Exception("User not found");

			ArgumentNullException.ThrowIfNull(auditLogSource);

			if (!isAdministrator)
				CheckIsNotLastAdmin(id);

			var oldUser = new Core.Entities.User(user);
			string[] oldRoles = (await this.userManager.GetRolesAsync(user)).ToArray();

			user.IsAdministrator = isAdministrator;

			var updateUserIdentityResult = await this.userManager.UpdateAsync(user);
			AssertSuccess(updateUserIdentityResult);

			await this.auditLogAdministration.LogUpdateUserAsync(
				oldUser,
				oldRoles,
				user,
				auditLogSource,
				auditChangedByUserId
			);
		}

		public async Task DeleteUserAsync(string id, string auditLogSource, string auditChangedByUserId)
		{
			var user = await this.userManager.FindByIdAsync(id);
			if (user == null)
				throw new Exception("User not found");

			ArgumentNullException.ThrowIfNull(auditLogSource);

			var oldUser = new Core.Entities.User(user);
			string[] oldRoles = (await this.userManager.GetRolesAsync(user)).ToArray();

			var deleteUserIdentityResult = await this.userManager.DeleteAsync(user);
			AssertSuccess(deleteUserIdentityResult);

			await this.auditLogAdministration.LogDeleteUserAsync(
				oldUser,
				oldRoles,
				auditLogSource,
				auditChangedByUserId
			);
		}

		private void CheckRequiredField(string fieldName, string fieldValue)
		{
			if (string.IsNullOrWhiteSpace(fieldValue))
				throw new Exception($"[{fieldName}] is required");
		}

		private void CheckIsNotLastAdmin(string userId)
		{
			bool isLastAdmin = this.userManager.Users.Where(u => u.IsAdministrator).Count() == 1 &&
				this.userManager.Users.Where(u => u.IsAdministrator).Single().Id == userId;

			if (isLastAdmin)
				throw new Exception("This user is the only administrator and can therefore not be removed as administrator from the system.");
		}

		private void CheckUserDoesNotExistByEmail(string email, string id = null)
		{
			var existingUser = this.userManager.FindByEmailAsync(email).Result;
			if (existingUser != null && (string.IsNullOrEmpty(id) || existingUser.Id != id))
				throw new Exception($"The email [{email}] already exists");
		}

		private void CheckUserDoesNotExistByUniqueId(string uniqueId)
		{
			var existingUser = this.userManager.Users.SingleOrDefault(u => u.UniqueId == uniqueId);
			if (existingUser != null)
				throw new Exception($"The user with unique ID {uniqueId} already exists");
		}
	}
}
