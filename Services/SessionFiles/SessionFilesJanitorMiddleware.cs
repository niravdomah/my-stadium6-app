using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Twenty57.Stadium.WebApp.Spa.Core.Interfaces;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.SessionFiles
{
	public class SessionFilesJanitorMiddleware
	{
		private readonly RequestDelegate next;
		private readonly ISessionFilesJanitor sessionFilesJanitor;

		public SessionFilesJanitorMiddleware(RequestDelegate next, ISessionFilesJanitor sessionFilesJanitor)
		{
			this.next = next;
			this.sessionFilesJanitor = sessionFilesJanitor;
		}

		public async Task InvokeAsync(HttpContext httpContext)
		{
			this.sessionFilesJanitor.RefreshTimeout(httpContext.Session.Id);

			await this.next(httpContext);
		}
	}
}
