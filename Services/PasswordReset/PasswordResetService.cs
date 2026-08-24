using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using System;
using Twenty57.Stadium.Core;
using Twenty57.Stadium.Core.Web.Helpers;
using Twenty57.Stadium.WebApp.Spa.Administration.Interfaces;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Configuration;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.PasswordReset
{
	public class PasswordResetService
	{
		private readonly Config config;
		private readonly ISmtpSettingsStore smtpSettingsStore;
		private readonly IWebHostEnvironment webHostEnvironment;

		public PasswordResetService(IOptionsMonitor<Config> configOptions, ISmtpSettingsStore smtpSettingsStore, IWebHostEnvironment webHostEnvironment)
		{
			this.config = configOptions.CurrentValue;
			this.smtpSettingsStore = smtpSettingsStore;
			this.webHostEnvironment = webHostEnvironment;
		}

		public SmtpSettings GetSmtpSettings()
		{
			var dbSettings = this.smtpSettingsStore.GetAsync().Result;
			return dbSettings == null ? null :
				new SmtpSettings
				{
					EnableSsl = dbSettings.EnableSsl,
					From = dbSettings.FromAddress,
					Password = dbSettings.SmtpPassword,
					Port = int.TryParse(dbSettings.SmtpPort, out int result) ? result : null,
					Server = dbSettings.SmtpServer,
					Username = dbSettings.SmtpUsername
				};
		}

		public bool AreSmtpSettingsValid(SmtpSettings smtpSettings)
		{
			return smtpSettings != null
				&& !string.IsNullOrEmpty(smtpSettings.From)
				&& !string.IsNullOrEmpty(smtpSettings.Server)
				&& !string.IsNullOrEmpty(smtpSettings.Username)
				&& !string.IsNullOrEmpty(smtpSettings.Password)
				&& smtpSettings.Port != null && smtpSettings.Port > 0;
		}

		public void SendPasswordResetEmail(string email, string resetUrl)
		{
			var smtpSettings = GetSmtpSettings();
			if (!AreSmtpSettingsValid(smtpSettings))
				throw new Exception("Invalid SMTP settings.");

			PasswordResetEmailHelper.SendPasswordResetEmail(
				smtpSettings,
				email,
				resetUrl,
				this.config.WebAppName,
				this.webHostEnvironment.WebRootPath,
				this.config.SmtpDeliveryMethod,
				this.config.SmtpPickupDirectoryLocation
			);
		}
	}
}
