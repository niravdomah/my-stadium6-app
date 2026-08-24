using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Configuration;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.WebApi.Controllers
{
	[ApiController]
	[Area("WebApi")]
	[Route("api/debug-mode")]
	[AllowAnonymous]
	[WebApiKeyValidation]
	public class DebugModeController : ControllerBase
	{
		// GET: api/debug-mode?key=key
		[HttpGet]
		public IActionResult GetDebugMode(string key)
		{
			return Ok(AppSettingsHelper.GetInstance().AppSettings.Config.Debug);
		}

		// PUT: api/debug-mode?key=key
		[HttpPut]
		public IActionResult SetDebugMode(string key, [FromBody] bool debugMode)
		{
			AppSettingsHelper.ExecuteAndSave(s =>
			{
				s.AppSettings.Config.Debug = debugMode;

				if (s.AppSettings.Serilog is JObject serilogSection
					&& serilogSection["MinimumLevel"]?["Override"] is JObject overrideSection
					&& overrideSection["Twenty57"] != null)
				{
					overrideSection["Twenty57"] = debugMode ? "Information" : "Warning";
				}
			});

#if !DEBUG
			Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = AppSettingsHelper.GetInstance().AppSettings.Config.Debug;
#endif

			return Ok();
		}
	}
}
