using System.Collections.Generic;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.Authentication.OAuth
{
	internal class UserInfo
	{
		public string UniqueId { get; set; }

		public string Email { get; set; }

		public string Name { get; set; }

		public List<string> Roles { get; set; } = new List<string>();
	}
}
