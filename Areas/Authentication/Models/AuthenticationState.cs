using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Twenty57.Stadium.WebApp.Spa.Core.Entities;
using Twenty57.Stadium.WebApp.Spa.Core.Enums;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Authentication.Models
{
	[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
	public class AuthenticationState
	{
		public AuthenticationType AuthenticationType { get; set; }

		public UserClaims UserClaims { get; set; }

		public AuthenticationSettings AuthenticationSettings { get; set; }
	}
}
