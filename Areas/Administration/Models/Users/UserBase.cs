using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Twenty57.Stadium.WebApp.Spa.Template.Areas.Administration.Models.Users.Validation;
using Twenty57.Stadium.WebApp.Spa.Template.Models.Validation;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Administration.Models.Users
{
	public class UserBase
	{
		[Required]
		[Email]
		[UniqueEmail]
		public string Email { get; set; }

		public string Name { get; set; }

		[IsValidWindowsUserName]
		public string UserName { get; set; }

		public virtual string Password { get; set; }

		public bool IsAdministrator { get; set; }

		[HasDefaultRole]
		public IEnumerable<string> Roles { get; set; }
	}
}
