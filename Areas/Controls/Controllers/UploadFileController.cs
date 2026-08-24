using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileSystemGlobbing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Twenty57.Stadium.WebApp.Spa.Core.Entities.FileTokens;
using Twenty57.Stadium.WebApp.Spa.Core;
using Twenty57.Stadium.WebApp.Spa.Core.Interfaces;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Controls.Controllers
{
	[ApiController]
	[Authorize(Policies.UserExistsAccess)]
	[Area("Controls")]
	[Route("api/[area]/[controller]/[action]")]
	public class UploadFileController : ControllerBase
	{
		private readonly IFilesService filesService;
		private readonly IControlPropertyValueProvider controlPropertyValueProvider;

		public UploadFileController(
			IFilesService filesService,
			IControlPropertyValueProvider controlPropertyValueProvider)
		{
			this.filesService = filesService;
			this.controlPropertyValueProvider = controlPropertyValueProvider;
		}

		// POST: api/Controls/UploadFile/Upload?controlId=[controlId]
		[HttpPost]
		public IActionResult Upload(string controlId, List<IFormFile> file)
		{
			string status = string.Empty;
			SessionFileToken fileToken = null;
			bool isSuccess = false;

			try
			{
				var formFile = file.Single();

				string[] allowedExtensions = GetAllowedExtensions(controlId);
				if (!IsFileExtensionAllowed(formFile.FileName, allowedExtensions))
				{
					throw new Exception(GetInvalidFileExtensionErrorMessage(formFile.FileName, allowedExtensions));
				}

				fileToken = this.filesService.CreateFileToken(formFile.OpenReadStream(), Path.GetFileName(formFile.FileName));

				isSuccess = true;
			}
			catch (Exception exception)
			{
				status = exception.Message;
			}

			return Ok(new
			{
				isSuccess,
				status,
				fileToken
			});
		}

		// POST: api/Controls/UploadFile/Delete?fileToken=[fileToken]
		[HttpPost]
		public IActionResult Delete(string fileToken)
		{
			string status = string.Empty;
			bool isSuccess = false;

			try
			{
				if (this.filesService.TryParseFileToken(fileToken, out var parsedFileToken))
				{
					System.IO.File.Delete(parsedFileToken.GetFullPath());

					isSuccess = true;
				}
				else
				{
					status = "Unable to delete file";
				}
			}
			catch (Exception exception) when (!(exception is DirectoryNotFoundException))
			{
				status = exception.Message;
			}

			return Ok(new
			{
				isSuccess,
				status
			});
		}

		private string[] GetAllowedExtensions(string controlId)
		{
			string allowedExtensions = this.controlPropertyValueProvider.GetPropertyValue<string>(controlId, "AllowedExtensions");

			if (string.IsNullOrEmpty(allowedExtensions))
				return Array.Empty<string>();

			return allowedExtensions.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
									.Select(e => e.Trim().TrimStart('.'))
									.ToArray();
		}

		private bool IsFileExtensionAllowed(string fileName, string[] allowedExtensions)
		{
			if (!allowedExtensions.Any())
				return true;

			if (!fileName.Contains("."))
				return false;

			string fileExtension = Path.GetExtension(fileName).TrimStart('.');

			var matcher = new Matcher();
			matcher.AddIncludePatterns(allowedExtensions);

			return matcher.Match(fileExtension).HasMatches;
		}

		private string GetInvalidFileExtensionErrorMessage(string fileName, string[] allowedExtensions)
		{
			string fileExtension = Path.GetExtension(fileName).TrimStart('.');
			string errorMessage = $"Unallowed file extension '{fileExtension}'. Allowed extension";

			if (allowedExtensions.Length > 1)
				errorMessage += "s";

			errorMessage += $": {string.Join(", ", allowedExtensions.Select(e => $".{e}"))}";

			return errorMessage;
		}
	}
}
