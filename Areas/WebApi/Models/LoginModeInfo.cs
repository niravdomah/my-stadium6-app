using Twenty57.Stadium.WebApp.Spa.Core.Enums;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.WebApi.Models
{
	public class LoginModeInfo
	{
		public string Email { get; set; }

		public string Password { get; set; }

		public string Name { get; set; }

		public string UserName { get; set; }

		public string UniqueId { get; set; }

		public AuthenticationType NewSecurityMode { get; set; }

		public string RedirectUrl { get; set; }

		public string LogoutRedirectUrl { get; set; }

		public OidcProvider? OidcProvider { get; set; }

		public string AuthenticationAuthorityUri { get; set; }

		public string Tenant { get; set; }

		public string ClientID { get; set; }

		public string ApiResourceName { get; set; }

		public string ApiResourceSecret { get; set; }

		public string RoleClaimName { get; set; }

		public string Audience { get; set; }

		public string Scopes { get; set; }
	}
}
