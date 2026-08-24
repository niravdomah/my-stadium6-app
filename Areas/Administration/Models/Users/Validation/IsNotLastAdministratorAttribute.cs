using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Twenty57.Stadium.WebApp.Spa.Administration.Interfaces;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Administration.Models.Users.Validation
{
	public class IsNotLastAdministratorAttribute : ValidationAttribute
	{
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			var userStore = validationContext.GetService<IUserStore>();
			var administrators = userStore.Users.Where(u => u.IsAdministrator);
			if (administrators.Count() > 1)
				return ValidationResult.Success;

			if (validationContext.ObjectInstance is IIsAdministratorToBeEdited userToBeEdited)
			{
				bool newIsAdministratorStatus = userToBeEdited.IsAdministrator;
				if (newIsAdministratorStatus != false)
					return ValidationResult.Success;
			}

			string userId = (string)value;
			var lastAdministrator = administrators.Single();
			if (lastAdministrator.Id != userId)
				return ValidationResult.Success;

			return new ValidationResult("This user is the only administrator and can therefore not be removed as administrator from the system");
		}
	}
}
