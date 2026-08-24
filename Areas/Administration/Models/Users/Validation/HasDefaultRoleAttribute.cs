using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Twenty57.Stadium.WebApp.Spa.Core.Entities;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Administration.Models.Users.Validation
{
	public class HasDefaultRoleAttribute : ValidationAttribute
	{
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			var roles = (IEnumerable<string>)value;
			string defaultRole = roles.SingleOrDefault(r => r == Role.DefaultRoleName);

			if (defaultRole != null)
				return ValidationResult.Success;

			return new ValidationResult($"The '{Role.DefaultRoleName}' default role was not found in roles");
		}
	}
}
