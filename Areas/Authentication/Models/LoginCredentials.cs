using System.ComponentModel.DataAnnotations;
using Twenty57.Stadium.WebApp.Spa.Template.Models.Validation;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Authentication.Models
{
	public class LoginCredentials
	{
		[Required]
		[Email]
		public string Email { get; set; }

		[Required]
		public string Password { get; set; }
	}
}
