using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Twenty57.Stadium.WebApp.Spa.Core.Enums;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Configuration;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.Authentication.Anonymous
{
	public class AnonymousAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
	{
		private readonly IOptionsMonitor<Config> configOptions;

		public AnonymousAuthenticationHandler(IOptionsMonitor<Config> configOptions, IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
			: base(options, logger, encoder)
		{
			this.configOptions = configOptions;
		}

		protected override Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			AuthenticateResult result;

			if (this.configOptions.CurrentValue.AuthenticationType == AuthenticationType.Anonymous)
			{
				string authenticationScheme = AuthenticationSchemes.GetDefaultAuthenticationScheme(AuthenticationType.Anonymous);
				var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(authenticationScheme));
				Context.User = claimsPrincipal;

				result = AuthenticateResult.Success(new AuthenticationTicket(Context.User, authenticationScheme));
			}
			else
			{
				result = AuthenticateResult.Fail($"Incorrect authentication type: {this.configOptions.CurrentValue.AuthenticationType}.");
			}

			return Task.FromResult(result);
		}
	}
}
