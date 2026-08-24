using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Configuration;
using Twenty57.Stadium.WebApp.Spa.Template.Areas.Application.Models;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Application.Controllers
{
	[ApiController]
	[Area("Application")]
	[Route("api/[area]/[controller]/[action]")]
	public class SessionVariablesController : ControllerBase
	{
		private readonly Config config;

		public SessionVariablesController(IOptionsSnapshot<Config> configOptions)
		{
			this.config = configOptions.Value;
		}

		// GET: api/Application/SessionVariables/All
		[HttpGet]
		public IActionResult All()
		{
			var sessionVariableNames = this.config.SessionVariableNames?.ToHashSet() ?? new HashSet<string>();

			var session = HttpContext.Session;
			var sessionVariables = session.Keys
										.Where(k => sessionVariableNames.Contains(k))
										.ToDictionary(k => k, k => session.GetString(k));

			return Ok(sessionVariables);
		}

		// POST: api/Application/SessionVariables/Set
		[HttpPost]
		public IActionResult Set(SessionVariable sessionVariable)
		{
			if (!this.config.SessionVariableNames.Contains(sessionVariable.Name))
				throw new Exception($"Invalid session variable: {sessionVariable.Name}");

			HttpContext.Session.SetString(sessionVariable.Name, sessionVariable.SerializedValue);

			return NoContent();
		}
	}
}
