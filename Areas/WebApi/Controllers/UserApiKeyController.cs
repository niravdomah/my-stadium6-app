using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using Twenty57.Stadium.WebApp.Spa.Administration;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.WebApi.Controllers
{
	[ApiController]
	[Area("WebApi")]
	[Route("api/web-api-key")]
	[AllowAnonymous]
	[WebApiKeyValidation]
	public class UserApiKeyController : ControllerBase
	{
		private readonly AdministrationContext administrationContext;

		public UserApiKeyController(AdministrationContext administrationContext)
		{
			this.administrationContext = administrationContext;
		}

		// GET: api/web-api-key?key=key
		[HttpGet]
		public IActionResult GetKey(string key)
		{
			return Ok(JsonConvert.SerializeObject(this.administrationContext.Application.WebApiKey));
		}

		// POST: api/web-api-key/generate?key=key
		[HttpPost]
		[Route("generate")]
		public IActionResult GenerateKey(string key)
		{
			string newUserApiKey = Guid.NewGuid().ToString();

			this.administrationContext.Application.WebApiKey = newUserApiKey;
			this.administrationContext.SaveChanges();

			return Ok(JsonConvert.SerializeObject(this.administrationContext.Application.WebApiKey));
		}

		// DELETE: api/we-api-key/clear?key=key
		[HttpPost]
		[Route("clear")]
		public IActionResult ClearKey(string key)
		{
			this.administrationContext.Application.WebApiKey = null;
			this.administrationContext.SaveChanges();

			return Ok(new
			{
				Message = "Web api key updated successfully"
			});
		}
	}
}
