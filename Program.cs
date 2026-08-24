using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using Twenty57.Stadium.WebApp.Spa.Administration;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Configuration;

namespace Twenty57.Stadium.WebApp.Spa.Template
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Must run before anything reads the storage location.
			bool storageIsEphemeral = ResolveStorageLocation(builder);

			// Must run before builder.Build() so the config watcher reads and watches the volume copy.
			bool configWasReset = RelocateWritableConfig(builder);

			var startup = new Startup(builder.Configuration);
			startup.ConfigureServices(builder.Services);

			builder.Host.UseSerilog((hostBuilderContext, loggerConfiguration) =>
			{
				loggerConfiguration.ReadFrom.Configuration(hostBuilderContext.Configuration);

				if (OperatingSystem.IsWindows())
					loggerConfiguration.WriteTo.EventLog(source: "Twenty57 Stadium", logName: "Application", manageEventSource: false);
			});

			var app = builder.Build();
			startup.Configure(app, builder.Environment);

			if (storageIsEphemeral)
			{
				app.Logger.LogWarning(
					"No storage volume configured ({EnvName} was not supplied by the environment or by configuration); " +
					"using ephemeral storage at {Path}. All data " +
					"(users, settings, encryption keys) will be lost on restart or redeploy. Configure a mounted " +
					"volume for production.",
					ConfigStorageLocation.AdministrationDbFolderEnvName,
					ConfigStorageLocation.VolumeDirectory);
			}

			if (configWasReset)
			{
				app.Logger.LogWarning(
					"ResetConfigToDefaults was set, so appsettings.json, web.config and administration.db in {Path} " +
					"were replaced from the image baseline; runtime configuration, users and audit history are gone. " +
					"Unset it or this repeats on every restart.",
					ConfigStorageLocation.VolumeDirectory);
			}

			// Must run after Configure so the DI container is available.
			DeployConfigSeeder.Seed(app);

#if DEBUG
			// Uncomment to create administration.db file
			CreateAdministrationDbIfNotExists(app);

			IdentityModelEventSource.ShowPII = true;
#else
			IdentityModelEventSource.ShowPII = Core.Services.Configuration.AppSettingsHelper.GetInstance().AppSettings.Config.Debug;
#endif

			app.Run();
		}

		// AdministrationDbFolder is read two ways - Startup through IConfiguration, ConfigStorageLocation straight
		// from the environment - so a value from any non-environment source (a --AdministrationDbFolder argument,
		// appsettings.json) reaches one and not the other. Resolve once here, then write it back to the
		// environment. Returns true when nothing supplied a value and the ephemeral fallback applied.
		private static bool ResolveStorageLocation(WebApplicationBuilder builder)
		{
			if (OperatingSystem.IsWindows())
				return false;

			string configuredDirectory = builder.Configuration[ConfigStorageLocation.AdministrationDbFolderEnvName]?.Trim();

			if (!string.IsNullOrEmpty(configuredDirectory))
			{
				if (!Path.IsPathRooted(configuredDirectory))
				{
					throw new InvalidOperationException(
						$"{ConfigStorageLocation.AdministrationDbFolderEnvName} must be an absolute path; got " +
						$"'{configuredDirectory}'. A relative path resolves against the application directory, so the " +
						$"mounted volume would be ignored and all data lost on restart.");
				}

				Environment.SetEnvironmentVariable(ConfigStorageLocation.AdministrationDbFolderEnvName, configuredDirectory);
				return false;
			}

			Environment.SetEnvironmentVariable(
				ConfigStorageLocation.AdministrationDbFolderEnvName,
				Path.Combine(AppContext.BaseDirectory, "App_Data"));
			return true;
		}

		// For Container deployments, relocate configuration files to the VolumeDirectory for persistence.
		// The app directory is rewritten on redeploy, so runtime changes would be lost.
		// Returns true when a reset was applied, so Main can warn once a logger exists.
		private static bool RelocateWritableConfig(WebApplicationBuilder builder)
		{
			string volumeDirectory = ConfigStorageLocation.VolumeDirectory;
			if (string.IsNullOrEmpty(volumeDirectory))
				return false;

			Directory.CreateDirectory(volumeDirectory);

			bool resetToDefaults = ReadResetFlag(builder);
			SeedConfigFile("appsettings.json", volumeDirectory, resetToDefaults);
			SeedConfigFile("web.config", volumeDirectory, resetToDefaults);
			SeedConfigFile("administration.db", volumeDirectory, resetToDefaults);

			AddVolumeAppSettings(builder, volumeDirectory);

			return resetToDefaults;
		}

		private static bool ReadResetFlag(WebApplicationBuilder builder)
		{
			string raw = builder.Configuration["ResetConfigToDefaults"]?.Trim();
			return raw == "1" || string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase);
		}

		// After the last JSON source is the only position that works: appending (what AddJsonFile does) shadows
		// the environment and command-line sources, and inserting any earlier puts this above the baked
		// appsettings.json - CreateBuilder registers the ASPNETCORE_/DOTNET_ environment sources ahead of the
		// JSON files - so the baked copy wins and every runtime edit the admin UI makes is lost.
		private static void AddVolumeAppSettings(WebApplicationBuilder builder, string volumeDirectory)
		{
			var source = new JsonConfigurationSource
			{
				Path = Path.Combine(volumeDirectory, "appsettings.json"),
				Optional = true,
				ReloadOnChange = true,
			};

			// Splits the rooted path into a provider plus a file name, as AddJsonFile does internally.
			source.ResolveFileProvider();

			IList<IConfigurationSource> sources = builder.Configuration.Sources;
			int insertAt = -1;

			for (int index = 0; index < sources.Count; index++)
			{
				if (sources[index] is JsonConfigurationSource)
					insertAt = index + 1;
			}

			if (insertAt < 0)
				sources.Add(source);
			else
				sources.Insert(insertAt, source);
		}

		private static void SeedConfigFile(string fileName, string volumeDirectory, bool resetToDefaults)
		{
			string targetPath = Path.Combine(volumeDirectory, fileName);
			string sourcePath = Path.Combine(AppContext.BaseDirectory, fileName);

			if (!resetToDefaults && File.Exists(targetPath))
				return;

			if (!File.Exists(sourcePath))
				return;

			string stagingPath = targetPath + ".seeding";

			File.Copy(sourcePath, stagingPath, overwrite: true);
			File.Move(stagingPath, targetPath, overwrite: true);
		}

		private static void CreateAdministrationDbIfNotExists(IHost host)
		{
			using (var scope = host.Services.CreateScope())
			{
				var services = scope.ServiceProvider;
				try
				{
					var administrationContext = services.GetRequiredService<AdministrationContext>();
					administrationContext.Database.EnsureCreated();
				}
				catch (Exception exception)
				{
					var logger = services.GetRequiredService<ILogger<Program>>();
					logger.LogError(exception, "An error occured when creating the administration.db.");
				}
			}
		}
	}
}
