using Microsoft.Extensions.Options;
using Twenty57.Stadium.Core;
using Twenty57.Stadium.WebApp.Spa.Administration.Interfaces;
using Twenty57.Stadium.WebApp.Spa.Core.Interfaces;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Configuration;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.Connectors
{
	public class ConnectorsService : IConnectorsService
	{
		private readonly Config config;
		private readonly ISettingStore settingStore;
		private readonly IConnectionStore connectionStore;

		public ConnectorsService(
			IOptionsSnapshot<Config> configOptions,
			ISettingStore settingStore,
			IConnectionStore connectionStore)
		{
			this.config = configOptions.Value;
			this.settingStore = settingStore;
			this.connectionStore = connectionStore;
		}

		public string GetConnectorName(string connectionId)
		{
			return this.connectionStore.FindByIdAsync(connectionId).Result?.Name;
		}

		public string GetConnectionString(string connectionId)
		{
			string connectionString = this.connectionStore.FindByIdAsync(connectionId).Result?.ConnectionString;
			if (!TryResolveConnectionString(connectionString, out string resolvedConnectionString, out string errorMessage))
				throw new DoNotReportErrorException(errorMessage);

			return resolvedConnectionString;
		}

		public bool TryResolveConnectionString(string connectionString, out string resolvedConnectionString, out string errorMessage)
		{
			string additionalInfoErrorMessage = !this.config.Preview
				? " To fix it, go to the Connections menu of your application in SAM."
				: string.Empty;

			if (connectionString.StartsWith("= Settings."))
			{
				string settingName = connectionString.Trim().Replace("= Settings.", string.Empty);
				resolvedConnectionString = this.settingStore.FindByNameAsync(settingName).Result?.Value;

				if (resolvedConnectionString == null)
				{
					errorMessage = $"'Settings.{settingName}' was not found.{additionalInfoErrorMessage}";
					return false;
				}

				errorMessage = null;
				return true;
			}

			if (connectionString.StartsWith("="))
			{
				resolvedConnectionString = null;
				errorMessage = $"Expressions are limited to using Settings.{additionalInfoErrorMessage}";
				return false;
			}

			errorMessage = null;
			resolvedConnectionString = connectionString;
			return true;
		}
	}
}
