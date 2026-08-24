using System.ComponentModel.DataAnnotations;
using Twenty57.Stadium.WebApp.Spa.Template.Models.Validation;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Authentication.Models.Password
{
	public class PasswordResetInfo
	{
		[Required]
		[Email]
		public string Email { get; set; }

		public bool IsResend { get; set; }

		[Required]
		[Url]
		public string PasswordResetUrl { get; set; }
	}
}
