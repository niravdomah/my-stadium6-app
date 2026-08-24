using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Twenty57.Stadium.WebApp.Spa.Core.JsonConverters;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Configuration;
using Twenty57.Stadium.WebApp.Spa.Template.Areas.Application.Models;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Application.Controllers
{
	[ApiController]
	[Area("Application")]
	[Route("api/[area]/[controller]/[action]")]
	public class ConfigController : ControllerBase
	{
		private readonly Config config;

		public ConfigController(IOptionsSnapshot<Config> configOptions)
		{
			this.config = configOptions.Value;
		}

		// GET: api/Application/Config/All
		[HttpGet]
		public IActionResult All()
		{
			return Ok(new ClientConfig
			{
				DateTimeSerializationFormat = JsonSerializerFormats.DateTime,
				TimeSpanSerializationFormat = JsonSerializerFormats.TimeSpan,
				DateFormat = this.config.DateFormat,
				TimeFormat = this.config.TimeFormat,
				DataGridDisplayFormats = this.config.DataGridDisplayFormats,
				MaxFileUploadSizeInKB = (int)(WebConfigHelper.GetInstance().GetMaxAllowedContentLength() / 1024)
			});
		}
	}
}
