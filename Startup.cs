using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Serilog;
using System.Data.Common;
using System.IO;
using Twenty57.Stadium.Core.Web.Extensions;
using Twenty57.Stadium.WebApp.Spa.Administration;
using Twenty57.Stadium.WebApp.Spa.Administration.Interfaces;
using Twenty57.Stadium.WebApp.Spa.Administration.Stores;
using Twenty57.Stadium.WebApp.Spa.Core.Interfaces;
using Twenty57.Stadium.WebApp.Spa.Core.JsonConverters;
using Twenty57.Stadium.WebApp.Spa.Core.Services.CodeInterpreter;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Configuration;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Database;
using Twenty57.Stadium.WebApp.Spa.Core.Services.DataGridFilter;
using Twenty57.Stadium.WebApp.Spa.Core.Services.ValueResolver;
using Twenty57.Stadium.WebApp.Spa.Core.Services.WebService;
using Twenty57.Stadium.WebApp.Spa.Template.Services.Administration;
using Twenty57.Stadium.WebApp.Spa.Template.Services.Authentication;
using Twenty57.Stadium.WebApp.Spa.Template.Services.Authentication.Anonymous;
using Twenty57.Stadium.WebApp.Spa.Template.Services.Authentication.Cookies;
using Twenty57.Stadium.WebApp.Spa.Template.Services.Authentication.OAuth;
using Twenty57.Stadium.WebApp.Spa.Template.Services.Authentication.Windows;
using Twenty57.Stadium.WebApp.Spa.Template.Services.Authorization;
using Twenty57.Stadium.WebApp.Spa.Template.Services.Connectors;
using Twenty57.Stadium.WebApp.Spa.Template.Services.ControlPropertyValueProvider;
using Twenty57.Stadium.WebApp.Spa.Template.Services.DebugLogger;
using Twenty57.Stadium.WebApp.Spa.Template.Services.Error;
using Twenty57.Stadium.WebApp.Spa.Template.Services.Files;
using Twenty57.Stadium.WebApp.Spa.Template.Services.FolderPath;
using Twenty57.Stadium.WebApp.Spa.Template.Services.PasswordReset;
using Twenty57.Stadium.WebApp.Spa.Template.Services.RequestLimits;
using Twenty57.Stadium.WebApp.Spa.Template.Services.SessionFiles;
using Twenty57.Stadium.WebApp.Spa.Template.Services.SessionVariables;
using Twenty57.Stadium.WebApp.Spa.Template.Services.Spa;
using Twenty57.Stadium.WebApp.Spa.Template.Services.User;

namespace Twenty57.Stadium.WebApp.Spa.Template
{
	public class Startup
	{
		private readonly IConfiguration configuration;

		public const string AdministrationDbFolderConfigurationName = "AdministrationDbFolder";

		public Startup(IConfiguration configuration)
		{
			this.configuration = configuration;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			var configSection = this.configuration.GetSection(Config.ConfigSectionName);
			var config = configSection.Get<Config>();
			var httpContextAccessor = new HttpContextAccessor();

			services.Configure<Config>(configSection);

			services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);

			services.AddTransient<IFolderPathService, FolderPathService>();

			services.AddDbContext<AdministrationContext>(o => o.UseSqlite(GetAdministrationDbConnectionString(services)))
					.AddScoped<IUserStore, UserStore>()
					.AddScoped<IRoleStore, RoleStore>()
					.AddScoped<IPageStore, PageStore>()
					.AddScoped<IAuditLogStore, AuditLogStore>()
					.AddScoped<ISettingStore, SettingStore>()
					.AddScoped<ISmtpSettingsStore, SmtpSettingsStore>()
					.AddScoped<IAuthenticationSettingStore, AuthenticationSettingStore>()
					.AddScoped<IConnectionStore, ConnectionStore>()
					.AddScoped<IUserAdministration, UserAdministration>()
					.AddScoped<IRoleAdministration, RoleAdministration>()
					.AddScoped<IPageAdministration, PageAdministration>()
					.AddScoped<IAuditLogAdministration, AuditLogAdministration>();

			services.AddCookieAuthentication()
					.AddAnonymousAuthentication()
					.AddOAuthAuthentication()
					.AddWindowsAuthentication();

			services.AddDynamicAuthentication();

			services.AddCustomAuthorization();

			services.AddTransient<IUserService, UserService>();
			services.AddTransient<PasswordResetService>();

			services.AddSessionVariables(config.WebAppId);
			services.AddSingleton<ISessionFilesJanitor, SessionFilesJanitor>();

			services.AddSignalR()
					.AddNewtonsoftJsonProtocol();

			services.AddTransient<IFilesService, FilesService>();

			services.AddTransient<ICodeInterpreter, CodeInterpreter>();

			services.AddTransient<IControlPropertyValueProvider, ControlPropertyValueProvider>();

			services.AddControllersWithViews()
					.AddNewtonsoftJson(o =>
					{
						o.SerializerSettings.ContractResolver = new DefaultContractResolver();  // prevents automatic camelCasing of property names
						o.SerializerSettings.Converters.Add(new StringEnumConverter());
						o.SerializerSettings.Converters.Add(new DateTimeConverter());
						o.SerializerSettings.Converters.Add(new TimeSpanConverter());
						o.SerializerSettings.Converters.Add(new ByteArrayConverter(services.BuildServiceProvider().GetRequiredService<IFilesService>()));
						JsonConvert.DefaultSettings = () => o.SerializerSettings;
					});

			services.AddAntiforgery(o => o.HeaderName = AntiforgeryConstants.AntiforgeryTokenHeaderName);

			ConfigureDataProtection(services);

			services.Configure<KestrelServerOptions>(o => o.Limits.MaxRequestBodySize = null);
			services.Configure<FormOptions>(o =>
			{
				o.ValueLengthLimit = int.MaxValue;
				o.MultipartBodyLengthLimit = int.MaxValue;
			});

			if (!string.IsNullOrEmpty(this.configuration[AdministrationDbFolderConfigurationName]))
				services.AddSingleton<WebConfigSettingsCache>();

			services.AddDataGridFilter();

			services.AddTransient<IValueResolver, ValueResolver>();
			services.AddTransient<IConnectorsService, ConnectorsService>();
			services.AddTransient<IDatabase, Database>();
			services.AddTransient<IWebService, WebService>();

			DbProviderFactories.RegisterFactory("Microsoft.Data.SqlClient", Microsoft.Data.SqlClient.SqlClientFactory.Instance);
			DbProviderFactories.RegisterFactory("System.Data.Odbc", System.Data.Odbc.OdbcFactory.Instance);
			DbProviderFactories.RegisterFactory("Oracle.ManagedDataAccess.Client", Oracle.ManagedDataAccess.Client.OracleClientFactory.Instance);
		}

		public void Configure(IApplicationBuilder app, IHostEnvironment env)
		{
			app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

			// Must be before UseStaticFiles so the body-size limit is set before the body is read 
			// and the header callback is registered before any downstream producer (static files,
			// endpoints, SPA fallback) starts a response.
			if (!string.IsNullOrEmpty(this.configuration[AdministrationDbFolderConfigurationName]))
			{
				app.UseMiddleware<MaxRequestBodySizeMiddleware>();
				app.UseMiddleware<CustomResponseHeadersMiddleware>();
			}

			//if (env.IsDevelopment())
			//{
			//	//app.UseDeveloperExceptionPage();
			//}
			//else
			//{
			//	app.UseHsts();
			//	app.UseHttpsRedirection();
			//}

			app.UseStaticFiles();

			app.UseSerilogRequestLogging();

			app.UseSession()
				.UseMiddleware<InitialiseSessionMiddleware>()
				.UseMiddleware<SessionFilesJanitorMiddleware>();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers().RequireAuthorization();
				endpoints.MapHub<DebugLoggerHub>("/hubs/debugLoggerHub");
			});

			app.UseSpa(env);
		}

		// The Data Protection key ring (auth cookies + antiforgery tokens) has no persistent location in a
		// Linux container, so it regenerates each start and prior cookies/tokens fail to decrypt. Persist it
		// to the mounted volume, gated on the container-only AdministrationDbFolder setting.
		private void ConfigureDataProtection(IServiceCollection services)
		{

			string databaseDirectoryPath = this.configuration[AdministrationDbFolderConfigurationName];

			if (string.IsNullOrEmpty(databaseDirectoryPath))
				return;

			var keyRingDirectory = new DirectoryInfo(Path.Combine(databaseDirectoryPath, "DataProtection-Keys"));
			keyRingDirectory.Create();

			services.AddDataProtection().PersistKeysToFileSystem(keyRingDirectory);
		}

		private string GetAdministrationDbConnectionString(IServiceCollection services)
		{
			string databaseDirectoryPath = this.configuration[AdministrationDbFolderConfigurationName];

			if (string.IsNullOrEmpty(databaseDirectoryPath))
			{
				var folderPathService = services.BuildServiceProvider()
												.GetRequiredService<IFolderPathService>();
				databaseDirectoryPath = folderPathService.WebAppPath;
			}

			return this.configuration.GetConnectionString(AdministrationContext.ConnectionStringName)
										.ReplacePlaceholder(databaseDirectoryPath);
		}
	}
}
