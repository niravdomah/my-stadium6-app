using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Twenty57.Stadium.WebApp.Spa.Template.Models.Validation;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.WebApi.Models
{
	[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
	public class UserInfo
	{
		[Email]
		public string Email { get; set; }

		public string Password { get; set; }

		public string Name { get; set; }

		[IsValidWindowsUserName]
		public string Username { get; set; }

		public bool? IsAdministrator { get; set; }

		public string[] Roles { get; set; }
	}
}
