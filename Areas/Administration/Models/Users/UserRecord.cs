using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Administration.Models.Users
{
	[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
	public class UserRecord
	{
		public string Id { get; set; }

		public string Email { get; set; }

		public string UserName { get; set; }

		public string Name { get; set; }

		public bool IsAdministrator { get; set; }

		public bool IsLastAdministrator { get; set; }

		public bool IsLoggedIn { get; set; }

		public IEnumerable<string> Roles { get; set; }
	}
}
