using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using Twenty57.Stadium.WebApp.Spa.Core.Entities;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Administration.Models.Users.Validation
{
	public class ExistsAttribute : ValidationAttribute
	{
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			string userId = (string)value;

			var userManager = validationContext.GetService<UserManager<User>>();
			var user = userManager.FindByIdAsync(userId).Result;

			if (user != null)
				return ValidationResult.Success;

			return new ValidationResult("User not found");
		}
	}
}
