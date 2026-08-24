using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using Twenty57.Stadium.Core.Helpers;
using Twenty57.Stadium.WebApp.Spa.Core.Interfaces;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Configuration;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.FolderPath
{
	public class FolderPathService : IFolderPathService
	{
		private readonly Config config;
		private readonly IWebHostEnvironment webHostEnvironment;
		private readonly IHttpContextAccessor httpContextAccessor;

		public FolderPathService(IOptions<Config> configOptions, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor)
		{
			this.config = configOptions.Value;
			this.webHostEnvironment = webHostEnvironment;
			this.httpContextAccessor = httpContextAccessor;
		}

		public string WebAppPath
		{
			get
			{
				var directory = new DirectoryInfo(AppContext.BaseDirectory);

				while (directory.Name != this.config.WebAppId)
				{
					directory = directory.Parent;

					if (directory == null)
						throw new Exception($"Unable to compute root path for web application {this.config.WebAppName}.");
				}

				return directory.FullName;
			}
		}

		public string WebRootPath => this.webHostEnvironment.WebRootPath;

		public string AppSettingsPath => ConfigStorageLocation.AppSettingsPath;

		public string WebConfigPath => ConfigStorageLocation.WebConfigPath;

		public string UpdatesFolderPath => Path.Combine(WebAppPath, "App_Data", "Updates");

		public string SessionsFolderPath => Path.Combine(FolderPathHelper.GetSessionsBaseFolderPath(), $"{this.config.WebAppId}_sessions");

		public string SessionIdFolderPath => Path.Combine(SessionsFolderPath, this.httpContextAccessor.HttpContext.Session.Id);
	}
}
