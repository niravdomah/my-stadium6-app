using System.ComponentModel.DataAnnotations;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Authentication.Models.Password
{
	public class ChangePasswordInfo
	{
		[Required]
		public string OldPassword { get; set; }

		[Required]
		public string NewPassword { get; set; }
	}
}
