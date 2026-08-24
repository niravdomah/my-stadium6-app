using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Twenty57.Stadium.Core;
using Twenty57.Stadium.WebApp.Spa.Administration.Interfaces;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Configuration;
using Twenty57.Stadium.WebApp.Spa.Template.Areas.WebApi.Models;
using SmtpSettings = Twenty57.Stadium.Core.SmtpSettings;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.WebApi.Controllers
{
	[ApiController]
	[Area("WebApi")]
	[Route("api/[controller]")]
	[AllowAnonymous]
	[WebApiKeyValidation]
	public class ConfigurationController : ControllerBase
	{
		private readonly ISmtpSettingsStore smtpSettingsStore;

		public ConfigurationController(ISmtpSettingsStore smtpSettingsStore)
		{
			this.smtpSettingsStore = smtpSettingsStore;
		}

		// GET: api/configuration/all?key=key
		[HttpGet]
		[Route("all")]
		public IActionResult Get(string key)
		{
			var webConfigHelper = WebConfigHelper.GetInstance();
			var appSettingsHelper = AppSettingsHelper.GetInstance();

			uint maxAllowedContentLengthInBytes = webConfigHelper.GetMaxAllowedContentLength();
			int maxAllowedContentLengthInKB = (int)(maxAllowedContentLengthInBytes / 1024);
			int sessionTimeout = appSettingsHelper.AppSettings.Config.SessionStateTimeout;

			return Ok(new
			{
				MaxRequestLength = maxAllowedContentLengthInKB,
				SessionTimeout = sessionTimeout,
				CustomHeaders = webConfigHelper.GetCustomHeaders()
												.Select(h => new CustomHeader()
												{
													Name = h.Key,
													Value = h.Value
												})
			});
		}

		// PUT: api/configuration/misc?key=key
		[HttpPut]
		[Route("misc")]
		public IActionResult EditMisc(string key, Misc misc)
		{
			WebConfigHelper.ExecuteAndSave(
				w => w.SetMaxAllowedContentLength(misc.MaxRequestLengthInKB)
			);

			AppSettingsHelper.ExecuteAndSave(
				a =>
				{
					if (a.AppSettings.Config.SessionStateTimeout != misc.SessionTimeout)
						a.AppSettings.Config.HasCustomSessionStateTimeout = true;

					a.AppSettings.Config.SessionStateTimeout = misc.SessionTimeout;
				}
			);

			return Ok();
		}

		// POST: api/configuration/custom-header?key=key
		[HttpPost]
		[Route("custom-header")]
		public IActionResult AddCustomHeader(string key, CustomHeader customHeader)
		{
			WebConfigHelper.ExecuteAndSave(
				w => w.AddCustomHeader(customHeader.Name, customHeader.Value)
			);

			return Ok();
		}

		// PUT: api/configuration/custom-header?key=key&name=customHeaderName
		[HttpPut]
		[Route("custom-header")]
		public IActionResult EditCustomHeader(string key, string name, CustomHeader customHeader)
		{
			WebConfigHelper.ExecuteAndSave(
				w => w.SetCustomHeader(name, customHeader.Name, customHeader.Value)
			);

			return Ok();
		}

		// DELETE: api/configuration/custom-header?key=key&name=customHeaderName
		[HttpDelete]
		[Route("custom-header")]
		public IActionResult DeleteCustomHeader(string key, string name)
		{
			WebConfigHelper.ExecuteAndSave(
				w => w.RemoveCustomHeader(name)
			);

			return Ok();
		}

		// POST: api/configuration/smtp?key=key
		[HttpPost]
		[Route("smtp")]
		public async Task<IActionResult> Smtp(string key, SmtpSettings smtpSettings)
		{
			try
			{
				var smtpSettingsEntity = await this.smtpSettingsStore.GetAsync();

				if (smtpSettingsEntity == null)
				{
					await this.smtpSettingsStore.AddAsync(new Core.Entities.SmtpSettings
					{
						FromAddress = smtpSettings?.From,
						SmtpServer = smtpSettings.Server,
						SmtpUsername = smtpSettings?.Username,
						SmtpPassword = smtpSettings?.Password,
						SmtpPort = smtpSettings?.Port?.ToString() ?? string.Empty
					});
				}
				else
				{
					smtpSettingsEntity.FromAddress = smtpSettings.From;
					smtpSettingsEntity.SmtpServer = smtpSettings.Server;
					smtpSettingsEntity.SmtpUsername = smtpSettings.Username;
					smtpSettingsEntity.SmtpPassword = smtpSettings.Password;
					smtpSettingsEntity.SmtpPort = smtpSettings.Port?.ToString() ?? string.Empty;

					await this.smtpSettingsStore.EditAsync(smtpSettingsEntity);
				}

				await this.smtpSettingsStore.SaveAsync();

				return Ok();
			}
			catch (Exception e)
			{
				Log.Instance.Warning("Updating SMTP settings failed: " + e.Message);
				return BadRequest(e);
			}
		}
	}
}
