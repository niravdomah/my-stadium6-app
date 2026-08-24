using System.ComponentModel.DataAnnotations;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Authentication.Models.Password
{
	public class UpdatePasswordInfo
	{
		[Required]
		public string UserId { get; set; }

		[Required]
		public string PasswordResetToken { get; set; }

		[Required]
		public string NewPassword { get; set; }
	}
}
