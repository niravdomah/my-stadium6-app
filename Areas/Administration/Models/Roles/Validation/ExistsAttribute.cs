using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using Twenty57.Stadium.WebApp.Spa.Core.Entities;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Administration.Models.Roles.Validation
{
	public class ExistsAttribute : ValidationAttribute
	{
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			string roleId = (string)value;

			var roleManager = validationContext.GetService<RoleManager<Role>>();
			var role = roleManager.FindByIdAsync(roleId).Result;

			if (role != null)
				return ValidationResult.Success;

			return new ValidationResult("Role not found");
		}
	}
}
