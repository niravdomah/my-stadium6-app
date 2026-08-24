using Microsoft.AspNetCore.SignalR;
using Twenty57.Stadium.WebApp.Spa.Core.Interfaces;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.DebugLogger
{
	public class DebugLoggerHub : Hub<IDebugLoggerClient>
	{
		public string GetConnectionId()
		{
			return Context.ConnectionId;
		}
	}
}
