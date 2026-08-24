using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Twenty57.Stadium.Core.Extensions;
using Twenty57.Stadium.Core.Helpers;
using Twenty57.Stadium.WebApp.Spa.Administration.Interfaces;
using Twenty57.Stadium.WebApp.Spa.Core;
using Twenty57.Stadium.WebApp.Spa.Core.Entities.FileTokens;
using Twenty57.Stadium.WebApp.Spa.Core.Interfaces;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Configuration;
using Twenty57.Stadium.WebApp.Spa.Core.Services.WebService;
using Twenty57.Stadium.WebApp.Spa.Template.Services.Authorization;
using Twenty57.Stadium.WebApp.Spa.Template.Services.DebugLogger;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Documents.Controllers.Templates
{
	[ApiController]
	[Authorize(Policies.RolesAccess)]
	[Area("Documents")]
	[PageUse("StartPage")]
	[Route("api/[area]/Templates/[controller]/[action]")]
	public class DefaultTemplateController : ControllerBase
	{
		private readonly Config config;
		private readonly IValueResolver valueResolver;
		private readonly IUserService userService;
		private readonly ISettingStore settingStore;
		private readonly IConnectorsService connectorsService;
		private readonly IDatabase database;
		private readonly IWebService webService;
		private readonly IFilesService filesService;
		private readonly ICodeInterpreter codeInterpreter;
		private readonly IHubContext<DebugLoggerHub, IDebugLoggerClient> debugLoggerHubContext;

		public DefaultTemplateController(
			IOptionsSnapshot<Config> configOptions,
			IHubContext<DebugLoggerHub, IDebugLoggerClient> debugLoggerHubContext,
			IValueResolver valueResolver,
			IUserService userService,
			ISettingStore settingStore,
			IConnectorsService connectorsService,
			IDatabase database,
			IWebService webService,
			IFilesService filesService,
			ICodeInterpreter codeInterpreter
		)
		{
			this.config = configOptions.Value;
			this.valueResolver = valueResolver;
			this.userService = userService;
			this.settingStore = settingStore;
			this.connectorsService = connectorsService;
			this.database = database;
			this.webService = webService;
			this.filesService = filesService;
			this.codeInterpreter = codeInterpreter;
			this.debugLoggerHubContext = debugLoggerHubContext;
		}

		
	}
}
