using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using Twenty57.Stadium.WebApp.Spa.Core.Entities;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Administration.Models.Roles.Validation
{
	public class UniqueNameAttribute : ValidationAttribute
	{
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			string name = ((string)value)?.Trim();

			var roleManager = validationContext.GetService<RoleManager<Role>>();
			var role = roleManager.FindByNameAsync(name).Result;

			if (role == null)
				return ValidationResult.Success;

			bool isCurrentRole = validationContext.ObjectInstance is RoleToEdit roleToEdit
									&& role.Id == roleToEdit.Id;

			if (isCurrentRole)
				return ValidationResult.Success;

			return new ValidationResult($"Name '{name}' is already taken.");
		}
	}
}
