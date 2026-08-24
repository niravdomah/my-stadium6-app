using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.IO;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Configuration;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.Spa
{
	public static class SpaMiddlewareExtensions
	{
		public static IApplicationBuilder UseSpa(this IApplicationBuilder app, IHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseWhen(
					httpContext => true,
					appBuilder => UseSpa("ClientApp/dist/debug", env, appBuilder)
				);
			}
			else
			{
				app.UseWhen(
					httpContext => IsDebug(),
					appBuilder => UseSpa("ClientApp/dist/debug", env, appBuilder)
				);

				app.UseWhen(
					httpContext => !IsDebug(),
					appBuilder => UseSpa("ClientApp/dist/production", env, appBuilder)
				);
			}

			return app;
		}

		private static void UseSpa(string spaRootPath, IHostEnvironment env, IApplicationBuilder app)
		{
			StaticFileOptions staticFileOptions = null;
			if (!env.IsDevelopment())
			{
				staticFileOptions = new StaticFileOptions
				{
					FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, spaRootPath))
				};
				app.UseStaticFiles(staticFileOptions);
			}

			app.MapWhen(x => !x.Request.Path.Value.StartsWith("/api"), builder =>
			{
				builder.UseSpa(spa =>
				{
					spa.Options.DefaultPage = "/index.html";

					if (env.IsDevelopment())
					{
						using (var scope = app.ApplicationServices.CreateScope())
						{
							var config = scope.ServiceProvider
												.GetRequiredService<IOptionsSnapshot<Config>>()
												.Value;

							spa.UseProxyToSpaDevelopmentServer(config.SpaDevHostUrl);
						}
					}
					else
					{
						spa.Options.SourcePath = "ClientApp";
						spa.Options.DefaultPageStaticFileOptions = staticFileOptions;
					}
				});
			});
		}

		private static bool IsDebug()
		{
			return AppSettingsHelper.GetInstance()
									.AppSettings
									.Config
									.Debug;
		}
	}
}
