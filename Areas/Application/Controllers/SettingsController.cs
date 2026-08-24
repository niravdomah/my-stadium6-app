using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Twenty57.Stadium.WebApp.Spa.Administration.Interfaces;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Application.Controllers
{
	[ApiController]
	[Area("Application")]
	[Route("api/[area]/[controller]/[action]")]
	public class SettingsController : ControllerBase
	{
		private readonly ISettingStore settingStore;
		// start of generated accessible settings
		private readonly ISet<string> accessibleSettingNames = new HashSet<string>();
		// end of generated accessible settings

		public SettingsController(ISettingStore settingStore)
		{
			this.settingStore = settingStore;
		}

		// GET: api/Application/Settings/All
		[HttpGet]
		public IActionResult All()
		{
			return Ok(
				this.settingStore.Settings
									.Where(s => this.accessibleSettingNames.Contains(s.Name))
									.ToDictionary(s => s.Name, s => s.Value)
			);
		}
	}
}
