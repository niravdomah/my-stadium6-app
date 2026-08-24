using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Twenty57.Stadium.WebApp.Spa.Core.Enums;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.Authentication.Anonymous
{
	public static class AnonymousAuthenticationServiceCollectionExtensions
	{
		public static IServiceCollection AddAnonymousAuthentication(this IServiceCollection services)
		{
			services.Configure<AuthenticationOptions>(o =>
			{
				string anonymousAuthenticationScheme = AuthenticationSchemes.GetDefaultAuthenticationScheme(AuthenticationType.Anonymous);
				o.AddScheme<AnonymousAuthenticationHandler>(anonymousAuthenticationScheme, anonymousAuthenticationScheme);
			});

			return services;
		}
	}
}
