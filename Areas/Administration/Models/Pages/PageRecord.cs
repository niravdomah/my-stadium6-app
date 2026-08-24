using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Administration.Models.Pages
{
	[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
	public class PageRecord
	{
		public string Id { get; set; }

		public string Name { get; set; }

		public bool IsStartPage { get; set; }

		public IEnumerable<string> MenuItems { get; set; }

		public IEnumerable<string> Roles { get; set; }
	}
}
