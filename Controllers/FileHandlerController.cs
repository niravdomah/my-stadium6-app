using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using Twenty57.Stadium.WebApp.Spa.Core.Interfaces;
using Twenty57.Stadium.WebApp.Spa.Template.Models.FileHandler;

namespace Twenty57.Stadium.WebApp.Spa.Template.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class FileHandlerController : ControllerBase
	{
		private readonly IFilesService filesService;

		public FileHandlerController(IFilesService filesService)
		{
			this.filesService = filesService;
		}

		// GET: api/FileHandler?content=content
		[HttpGet]
		public IActionResult GetFile(string content)
		{
			try
			{
				if (this.filesService.TryParseFileToken(content, out var fileToken))
				{
					var stream = this.filesService.GetInputStream(fileToken);
					return new FileStreamResult(stream, GetContentTypeForFile(fileToken.GetFullPath()));
				}
				else
				{
					byte[] contentData = Encoding.UTF8.GetBytes(content);
					return new FileContentResult(contentData, "application/octet-stream");
				}
			}
			catch (Exception exception)
			{
				throw new Exception($"Error reading file: {exception.Message}");
			}
		}

		// POST: api/FileHandler/Download
		[HttpPost]
		[Route("download")]
		public IActionResult DownloadFile([FromBody] DownloadFileViewModel viewModel)
		{
			try
			{
				string fileContentType = new FileExtensionContentTypeProvider().TryGetContentType(viewModel.FileName, out string contentType)
											? contentType
											: "application/octet-stream";

				if (this.filesService.TryParseFileToken(viewModel.Content, out var fileToken))
				{
					var stream = this.filesService.GetInputStream(fileToken);
					return new FileStreamResult(stream, fileContentType);
				}

				byte[] contentData;
				if (!HasTextFileExtension(viewModel.FileName) && viewModel.Content.StartsWith("["))
				{
					try
					{
						contentData = JsonConvert.DeserializeObject<byte[]>(viewModel.Content);
						return new FileContentResult(contentData, fileContentType);
					}
					catch (JsonReaderException)
					{ }
				}

				contentData = Encoding.UTF8.GetBytes(viewModel.Content);
				return new FileContentResult(contentData, fileContentType);
			}
			catch (Exception exception)
			{
				throw new Exception($"Error opening file: {exception.Message}");
			}
		}

		private bool HasTextFileExtension(string fileName)
		{
			string fileExtension = Path.GetExtension(fileName).ToLower();
			return (fileExtension == ".txt") || (fileExtension == ".json");
		}

		private string GetContentTypeForFile(string filePath = "")
		{
			if (!string.IsNullOrEmpty(filePath) && Path.GetExtension(filePath).ToLower() == ".svg")
				return "image/svg+xml";

			return "application/octet-stream";
		}
	}
}
