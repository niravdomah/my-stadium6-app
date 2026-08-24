using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.WebApi.Models
{
	[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
	public class ApplicationUpdate
	{
		public string Id { get; set; }

		public DateTime DateTime { get; set; }

		public string UserId { get; set; }

		public string Username { get; set; }

		public string DesignerVersion { get; set; }

		public bool FileExists { get; set; }
	}
}
