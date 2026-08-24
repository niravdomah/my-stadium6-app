using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.WebApi.Models
{
	[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
	public class User
	{
		public string Id { get; set; }

		public string Email { get; set; }

		public string Name { get; set; }

		public string Username { get; set; }

		public bool IsAdministrator { get; set; }

		public string[] Roles { get; set; }

		public object Data { get; set; }
	}
}
