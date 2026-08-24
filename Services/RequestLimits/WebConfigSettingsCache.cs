using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Configuration;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.RequestLimits
{
	public class WebConfigSettingsCache
	{
		private static string WebConfigPath => ConfigStorageLocation.WebConfigPath;

		private const uint DefaultMaxAllowedContentLength = 4194304;

		private readonly ILogger<WebConfigSettingsCache> logger;
		private readonly object gate = new object();
		private DateTime cachedWriteTimeUtc;
		private bool loaded;
		private uint maxAllowedContentLength;
		private IDictionary<string, string> customHeaders;

		public WebConfigSettingsCache(ILogger<WebConfigSettingsCache> logger)
		{
			this.logger = logger;
		}

		public uint GetMaxAllowedContentLength()
		{
			Refresh();
			return this.maxAllowedContentLength;
		}

		public IDictionary<string, string> GetCustomHeaders()
		{
			Refresh();
			return this.customHeaders;
		}

		private void Refresh()
		{
			DateTime writeTimeUtc = File.GetLastWriteTimeUtc(WebConfigPath);

			lock (this.gate)
			{
				if (this.loaded && writeTimeUtc == this.cachedWriteTimeUtc)
					return;

				try
				{
					var webConfigHelper = WebConfigHelper.GetInstance();
					uint parsedMaxAllowedContentLength = webConfigHelper.GetMaxAllowedContentLength();
					var parsedCustomHeaders = webConfigHelper.GetCustomHeaders();

					this.maxAllowedContentLength = parsedMaxAllowedContentLength;
					this.customHeaders = parsedCustomHeaders;
				}
				catch (Exception exception)
				{
					this.logger.LogError(
						exception,
						"Failed to read {WebConfigPath}; falling back to {Fallback} for request-limit and response-header enforcement. Fix the file to restore the configured values.",
						WebConfigPath,
						this.loaded ? "the last-known-good values" : "safe defaults");

					if (!this.loaded)
					{
						this.maxAllowedContentLength = DefaultMaxAllowedContentLength;
						this.customHeaders = new Dictionary<string, string>();
					}
				}

				this.cachedWriteTimeUtc = writeTimeUtc;
				this.loaded = true;
			}
		}
	}
}
