using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Authentication.Models
{
	[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
	public class AuthenticationSettings
	{
		public string OidcProvider { get; set; }

		public string Audience { get; set; }

		public string Authority { get; set; }

		public string Tenant { get; set; }

		public string RoleClaimName { get; set; }

		public string ResponseType { get; set; }

		public string RedirectUrl { get; set; }

		public string LogoutRedirectUrl { get; set; }

		public string ClientId { get; set; }

		public string Scope { get; set; }

		public string EndSessionEndpoint { get; set; }
	}
}
