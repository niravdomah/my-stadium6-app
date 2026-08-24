using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using Twenty57.Stadium.WebApp.Spa.Administration;
using Twenty57.Stadium.WebApp.Spa.Core.Interfaces;
using Twenty57.Stadium.WebApp.Spa.Template.Areas.WebApi.Models;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.WebApi.Controllers
{
	[ApiController]
	[Area("WebApi")]
	[Route("api/update-history")]
	[AllowAnonymous]
	[WebApiKeyValidation]
	public class UpdateHistoryController : ControllerBase
	{
		private readonly IFolderPathService folderPathService;
		private readonly AdministrationContext administrationContext;

		public UpdateHistoryController(IFolderPathService folderPathService, AdministrationContext administrationContext)
		{
			this.folderPathService = folderPathService;
			this.administrationContext = administrationContext;
		}

		// GET: api/update-history?key=key
		[HttpGet]
		public IActionResult GetUpdateHistory(string key)
		{
			var updates = new List<ApplicationUpdate>();

			foreach (var update in this.administrationContext.ApplicationUpdates)
			{
				updates.Add(new ApplicationUpdate()
				{
					Id = update.Id,
					DateTime = update.DateTime,
					UserId = update.UserId,
					Username = update.Username,
					DesignerVersion = update.DesignerVersion,
					FileExists = SapzUpdateFileExists(update.Id)
				});
			}

			return Ok(updates);
		}

		// GET: api/update-history/sapz?key=key&id=updateId
		[HttpGet]
		[Route("sapz")]
		public IActionResult GetSapz(string key, string id)
		{
			return new FileStreamResult(GetSapzFileStream(id), "application/octet-stream");
		}

		private string GetSapzPath(string id)
		{
			return Path.Combine(this.folderPathService.UpdatesFolderPath, $"{id}.sapz");
		}

		private FileStream GetSapzFileStream(string id)
		{
			if (!SapzUpdateFileExists(id))
				throw new FileNotFoundException("Sapz file update not found.");

			string sapzFilePath = GetSapzPath(id);
			return new FileStream(sapzFilePath, FileMode.Open, FileAccess.Read);
		}

		private bool SapzUpdateFileExists(string id)
		{
			string sapzFilePath = GetSapzPath(id);
			return System.IO.File.Exists(sapzFilePath);
		}
	}
}
