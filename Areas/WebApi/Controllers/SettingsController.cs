using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twenty57.Stadium.WebApp.Spa.Administration.Interfaces;
using Twenty57.Stadium.WebApp.Spa.Template.Areas.WebApi.Models;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.WebApi.Controllers
{
	[ApiController]
	[Area("WebApi")]
	[Route("api/[controller]")]
	[AllowAnonymous]
	[WebApiKeyValidation]
	public class SettingsController : ControllerBase
	{
		private readonly ISettingStore settingStore;

		public SettingsController(ISettingStore settingStore)
		{
			this.settingStore = settingStore;
		}

		// GET: api/settings?key=key
		[HttpGet]
		public IActionResult All(string key)
		{
			var settings = new List<Setting>();

			foreach (var setting in this.settingStore.Settings)
			{
				settings.Add(
					new Setting
					{
						ID = setting.Id,
						Name = setting.Name,
						Value = setting.Value,
						DefaultValue = setting.DefaultValue
					}
				);
			}

			return Ok(settings);
		}

		// PUT: api/settings?key=key
		[HttpPut]
		public async Task<IActionResult> Edit(string key, Setting setting)
		{
			var settingEntity = await this.settingStore.FindByNameAsync(setting.Name);
			settingEntity.Value = setting.Value;

			await this.settingStore.UpdateAsync(settingEntity);

			return Ok();
		}
	}
}
