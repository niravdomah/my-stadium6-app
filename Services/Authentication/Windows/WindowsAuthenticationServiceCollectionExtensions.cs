using Microsoft.Extensions.DependencyInjection;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.Authentication.Windows
{
	public static class WindowsAuthenticationServiceCollectionExtensions
	{
		public static IServiceCollection AddWindowsAuthentication(this IServiceCollection services)
		{
			//services.AddScoped<IClaimsTransformation, WindowsClaimsTransformer>();
			//services.AddAuthentication(AuthenticationSchemes.GetDefaultAuthenticationScheme(AuthenticationType.Windows))
			//	.AddNegotiate();

			return services;
		}
	}
}
