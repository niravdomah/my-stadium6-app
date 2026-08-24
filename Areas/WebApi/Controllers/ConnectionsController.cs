using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twenty57.Stadium.Core;
using Twenty57.Stadium.WebApp.Spa.Administration.Interfaces;
using Twenty57.Stadium.WebApp.Spa.Core.Interfaces;
using Twenty57.Stadium.WebApp.Spa.Template.Areas.WebApi.Models;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.WebApi.Controllers
{
	[ApiController]
	[Area("WebApi")]
	[Route("api/[controller]")]
	[AllowAnonymous]
	[WebApiKeyValidation]
	public class ConnectionsController : ControllerBase
	{
		private readonly IConnectionStore connectionStore;
		private readonly IConnectorsService connectorsService;

		public ConnectionsController(IConnectionStore connectionStore, IConnectorsService connectorsService)
		{
			this.connectionStore = connectionStore;
			this.connectorsService = connectorsService;
		}

		// GET: api/connections?key=key
		[HttpGet]
		public IActionResult All(string key)
		{
			var connections = new List<Connection>();

			foreach (var connection in this.connectionStore.Connections)
			{
				string errorMessage = this.connectorsService.TryResolveConnectionString(connection.ConnectionString, out _, out string error)
										? string.Empty
										: error;

				connections.Add(
					new Connection
					{
						ID = connection.Id,
						Name = connection.Name,
						ConnectionString = connection.ConnectionString,
						DefaultValue = connection.DefaultValue,
						ErrorMessage = errorMessage
					}
				);
			}

			return Ok(connections);
		}

		// PUT: api/connections?key=key
		[HttpPut]
		public async Task<IActionResult> Edit(string key, Connection connection)
		{
			var connectionEntity = await this.connectionStore.FindByNameAsync(connection.Name);
			connectionEntity.ConnectionString = connection.ConnectionString;

			await this.connectionStore.UpdateAsync(connectionEntity);

			if (!this.connectorsService.TryResolveConnectionString(connection.ConnectionString, out _, out string error))
				throw new DoNotReportErrorException(error);

			return Ok();
		}
	}
}
