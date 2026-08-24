using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Twenty57.Stadium.WebApp.Spa.Core.Enums;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Configuration;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.WebApi.Controllers
{
	[ApiController]
	[Area("WebApi")]
	[Route("api/api-info")]
	[AllowAnonymous]
	[WebApiKeyValidation]
	public class WebApiInfoController : ControllerBase
	{
		// GET: api/api-info/supported-features?key=key
		[HttpGet]
		[Route("supported-features")]
		public IActionResult GetSupportedFeatures(string key)
		{
			var features = new List<string>()
				{
					"Connections",
					"Settings",
					"LoginMode",
					"LoginModeV2",
					"UpdateHistory",
					"DownloadUpdateHistoryFiles",
					"DebugMode",
					"ConfigurationV2",
					"MiscConfiguration",
					"SMTPConfiguration",
					"AuditLog"
				};

			var appSettings = AppSettingsHelper.GetInstance()
												.AppSettings;

			if (appSettings.Config.AuthenticationType != AuthenticationType.Anonymous)
				features.Add("UserApi");

			return Ok(features);
		}
	}
}
