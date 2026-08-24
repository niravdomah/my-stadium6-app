using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Configuration;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Application.Models
{
	[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
	public class ClientConfig
	{
		public string DateTimeSerializationFormat { get; set; }

		public string TimeSpanSerializationFormat { get; set; }

		public string DateFormat { get; set; }

		public string TimeFormat { get; set; }

		public DataGridDisplayFormats DataGridDisplayFormats { get; set; }

		public int MaxFileUploadSizeInKB { get; set; }
	}
}
