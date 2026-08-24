using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Configuration;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.SessionVariables
{
	public static class SessionVariablesServiceCollectionExtensions
	{
		public static IServiceCollection AddSessionVariables(this IServiceCollection services, string webAppId)
		{
			services.AddDistributedMemoryCache();

			services.AddSession()
					.AddOptions<SessionOptions>()
					.Configure<IOptionsMonitor<Config>>((o, c) =>
					{
						o.Cookie.Name = webAppId;
						o.Cookie.IsEssential = true;
						o.IdleTimeout = TimeSpan.FromMinutes(c.CurrentValue.SessionStateTimeout);
					});

			return services;
		}
	}
}
