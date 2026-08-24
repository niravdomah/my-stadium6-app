using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Twenty57.Stadium.WebApp.Spa.Core;
using Twenty57.Stadium.WebApp.Spa.Core.Enums;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Configuration;
using Twenty57.Stadium.WebApp.Spa.Template.Services.Authorization.Requirements;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.Authorization
{
	public static class AuthorizationServiceCollectionExtensions
	{
		public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
		{
			services.AddAuthorization(o =>
			{
				o.AddPolicy(
					Policies.AdministratorAccess,
					p => p.Requirements.Add(new AdministratorAccessRequirement()));

				o.AddPolicy(
					Policies.RolesAccess,
					p => p.Requirements.Add(new RolesAccessRequirement()));

				o.AddPolicy(
					Policies.UserExistsAccess,
					p => p.Requirements.Add(new UserExistsAccessRequirement()));
			});

			services.AddOptions<AuthorizationOptions>()
				.Configure<IOptionsMonitor<Config>>((o, c) =>
				{
					o.FallbackPolicy = c.CurrentValue.AuthenticationType == AuthenticationType.Windows ? o.DefaultPolicy : null;
				});

			services.AddTransient<IAuthorizationHandler, AdministratorAccessHandler>()
					.AddTransient<IAuthorizationHandler, RolesAccessHandler>()
					.AddTransient<IAuthorizationHandler, UserExistsAccessHandler>();

			return services;
		}
	}
}
