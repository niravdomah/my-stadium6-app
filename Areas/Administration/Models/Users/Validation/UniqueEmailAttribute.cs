using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using Twenty57.Stadium.WebApp.Spa.Core.Entities;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Administration.Models.Users.Validation
{
	public class UniqueEmailAttribute : ValidationAttribute
	{
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			string email = (string)value;

			var userManager = validationContext.GetService<UserManager<User>>();
			var user = userManager.FindByEmailAsync(email).Result;

			if (user == null)
				return ValidationResult.Success;

			bool isCurrentUser = validationContext.ObjectInstance is UserToEdit userToEdit
									&& user.Id == userToEdit.Id;

			if (isCurrentUser)
				return ValidationResult.Success;

			return new ValidationResult($"Email '{email}' is already taken.");
		}
	}
}
