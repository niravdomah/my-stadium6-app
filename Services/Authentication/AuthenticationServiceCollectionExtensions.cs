using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.Authentication
{
	public static class AuthenticationServiceCollectionExtensions
	{
		public static IServiceCollection AddDynamicAuthentication(this IServiceCollection services)
		{
			services.AddSingleton<IAuthenticationSchemeProvider, CustomAuthenticationSchemeProvider>();

			services.Configure<AuthenticationOptions>(o =>
			{
				o.DefaultScheme = CustomAuthenticationSchemeProvider.DynamicDefaultAuthenticationScheme;
				o.DefaultAuthenticateScheme = CustomAuthenticationSchemeProvider.DynamicDefaultAuthenticationScheme;
				o.DefaultChallengeScheme = CustomAuthenticationSchemeProvider.DynamicDefaultAuthenticationScheme;
				o.DefaultForbidScheme = CustomAuthenticationSchemeProvider.DynamicDefaultAuthenticationScheme;
				o.DefaultSignInScheme = CustomAuthenticationSchemeProvider.DynamicDefaultAuthenticationScheme;
				o.DefaultSignOutScheme = CustomAuthenticationSchemeProvider.DynamicDefaultAuthenticationScheme;
			});

			return services;
		}
	}
}
