using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Configuration;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.Authentication
{
	public class CustomAuthenticationSchemeProvider : AuthenticationSchemeProvider
	{
		public const string DynamicDefaultAuthenticationScheme = "DynamicDefaultAuthenticationScheme";

		private readonly IOptionsMonitor<Config> configOptions;

		public CustomAuthenticationSchemeProvider(IOptionsMonitor<Config> configOptions, IOptions<AuthenticationOptions> options)
			: base(options)
		{
			this.configOptions = configOptions;
		}

		public override Task<AuthenticationScheme> GetSchemeAsync(string name)
		{
			if (name == DynamicDefaultAuthenticationScheme)
			{
				string authenticationScheme = AuthenticationSchemes.GetDefaultAuthenticationScheme(this.configOptions.CurrentValue.AuthenticationType);

				return base.GetSchemeAsync(authenticationScheme);
			}

			return base.GetSchemeAsync(name);
		}
	}
}
