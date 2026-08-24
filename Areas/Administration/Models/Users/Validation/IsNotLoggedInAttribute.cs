using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Administration.Models.Users.Validation
{
	public class IsNotLoggedInAttribute : ValidationAttribute
	{
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			string userId = (string)value;

			var httpContextAccessor = validationContext.GetService<IHttpContextAccessor>();
			var currentUser = httpContextAccessor.HttpContext.User;

			if (userId != currentUser.FindFirstValue(ClaimTypes.NameIdentifier))
				return ValidationResult.Success;

			return new ValidationResult(ErrorMessage);
		}
	}
}
