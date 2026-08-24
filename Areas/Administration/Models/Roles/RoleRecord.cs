using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Administration.Models.Roles
{
	[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
	public class RoleRecord
	{
		public string Id { get; set; }

		public string Name { get; set; }

		public bool IsDefault { get; set; }

		public IEnumerable<string> Pages { get; set; }
	}
}
