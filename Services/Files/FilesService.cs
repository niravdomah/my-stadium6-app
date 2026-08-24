using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using Twenty57.Stadium.Core;
using Twenty57.Stadium.Core.Helpers;
using Twenty57.Stadium.WebApp.Spa.Core.Entities.FileTokens;
using Twenty57.Stadium.WebApp.Spa.Core.Interfaces;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.Files
{
	public class FilesService : IFilesService
	{
		private readonly IFolderPathService folderPathService;
		private readonly IConnectorsService connectorsService;

		public FilesService(IFolderPathService folderPathService, IConnectorsService connectorsService)
		{
			this.folderPathService = folderPathService;
			this.connectorsService = connectorsService;
		}

		public bool TryParseFileToken(JObject fileTokenObject, out AFileToken fileToken)
		{
			if (fileTokenObject.TryGetValue("$type", out var type)
				&& type.ToString().StartsWith("Twenty57.Stadium.WebApp.Spa.Core.Entities.FileToken"))
			{
				return TryParseFileToken(JsonConvert.SerializeObject(fileTokenObject), out fileToken);
			}

			fileToken = null;
			return false;
		}

		public bool TryParseFileToken(string fileTokenJson, out AFileToken fileToken)
		{
			if ((fileTokenJson == null) || (!fileTokenJson.StartsWith("{")))
			{
				fileToken = null;
				return false;
			}

			try
			{
				var jsonSerializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
				{
					TypeNameHandling = TypeNameHandling.Objects,
					ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
					MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
				});

				using (var stringReader = new StringReader(fileTokenJson))
				using (var jsonTextReader = new JsonTextReader(stringReader))
					fileToken = jsonSerializer.Deserialize<AFileToken>(jsonTextReader);

				if (fileToken is FileSystemConnectorToken fileSystemConnectorToken)
				{
					fileSystemConnectorToken.InjectConnectorsService(this.connectorsService);
				}
				else if (fileToken is SessionFileToken sessionFileToken)
				{
					sessionFileToken.InjectFolderPathService(this.folderPathService);
				}
				else if (fileToken is EmbeddedFileToken embeddedFileToken)
				{
					embeddedFileToken.InjectFolderPathService(this.folderPathService);
				}

				return true;
			}
			catch (JsonException)
			{
				fileToken = null;
				return false;
			}
		}

		public SessionFileToken CreateFileToken(byte[] content)
		{
			using (var stream = new MemoryStream(content))
				return CreateFileToken(stream);
		}

		public SessionFileToken CreateFileToken(Stream stream, string fileName = null)
		{
			string sessionIdFolderPath = this.folderPathService.SessionIdFolderPath;

			Directory.CreateDirectory(sessionIdFolderPath);

			string fileId = Guid.NewGuid().ToString();
			string filePath = Path.Combine(sessionIdFolderPath, fileId);

			using (var fileStream = File.OpenWrite(filePath))
				stream.CopyTo(fileStream);

			return new SessionFileToken(fileId, fileName ?? string.Empty);
		}

		public Stream GetInputStream(AFileToken fileToken)
		{
			string path = fileToken.GetFullPath();

			var impersonationHelper = fileToken.GetImpersonationHelper();

			return RunImpersonated(impersonationHelper, () =>
			{
				CheckFileExists(path);
				return new ContextStream(File.OpenRead(path));
			});
		}

		public bool Exists(AFileToken fileToken)
		{
			var impersonationHelper = fileToken.GetImpersonationHelper();

			return RunImpersonated(impersonationHelper, () =>
			{
				return File.Exists(fileToken.GetFullPath());
			});
		}

		public string ReadText(AFileToken fileToken)
		{
			var impersonationHelper = fileToken.GetImpersonationHelper();

			return RunImpersonated(impersonationHelper, () =>
			{
				string path = fileToken.GetFullPath();
				CheckFileExists(path);
				return File.ReadAllText(path);
			});
		}

		public byte[] ReadBytes(AFileToken fileToken)
		{
			var impersonationHelper = fileToken.GetImpersonationHelper();

			return RunImpersonated(impersonationHelper, () =>
			{
				string path = fileToken.GetFullPath();
				CheckFileExists(path);
				return File.ReadAllBytes(path);
			});
		}

		public void Write(AFileToken fileToken, byte[] data)
		{
			var impersonationHelper = fileToken.GetImpersonationHelper();

			RunImpersonated(impersonationHelper, () =>
			{
				string path = fileToken.GetFullPath();
				CheckFileDoesNotExist(path);
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				File.WriteAllBytes(path, data);
			});
		}

		public void Delete(AFileToken fileToken)
		{
			var impersonationHelper = fileToken.GetImpersonationHelper();

			RunImpersonated(impersonationHelper, () =>
			{
				string path = fileToken.GetFullPath();
				if (File.Exists(path))
					File.Delete(path);
			});
		}

		public void CheckFileExists(AFileToken fileToken)
		{
			var impersonationHelper = fileToken.GetImpersonationHelper();

			RunImpersonated(impersonationHelper, () =>
			{
				CheckFileExists(fileToken.GetFullPath());
			});
		}

		private T RunImpersonated<T>(ImpersonationHelper impersonationHelper, Func<T> func)
		{
			if (impersonationHelper == null)
			{
				return func();
			}
			else
			{
				return impersonationHelper.RunImpersonated(func);
			}
		}

		private void RunImpersonated(ImpersonationHelper impersonationHelper, Action action)
		{
			if (impersonationHelper == null)
			{
				action();
			}
			else
			{
				impersonationHelper.RunImpersonated(action);
			}
		}

		private void CheckFileExists(string path)
		{
			if (!File.Exists(path))
				throw new Exception($"The file {Path.GetFileName(path)} could not be read because it does not exist.");
		}

		private void CheckFileDoesNotExist(string path)
		{
			if (File.Exists(path))
				throw new Exception($"The file {Path.GetFileName(path)} could not be written as it already exists.");
		}
	}
}
