using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.WebApi.Models
{
	[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
	public class Connection
	{
		public string ID { get; set; }

		public string Name { get; set; }

		public string ConnectionString { get; set; }

		public string DefaultValue { get; set; }

		public string ErrorMessage { get; set; }
	}
}
