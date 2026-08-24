using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.RequestLimits
{
	// Linux/Kestrel only : emits the web.config custom response headers that IIS emits on Windows.
	public class CustomResponseHeadersMiddleware
	{
		private readonly RequestDelegate next;
		private readonly WebConfigSettingsCache settingsCache;

		public CustomResponseHeadersMiddleware(RequestDelegate next, WebConfigSettingsCache settingsCache)
		{
			this.next = next;
			this.settingsCache = settingsCache;
		}

		public async Task InvokeAsync(HttpContext httpContext)
		{
			var customHeaders = this.settingsCache.GetCustomHeaders();

			httpContext.Response.OnStarting(() =>
			{
				foreach (var customHeader in customHeaders)
					httpContext.Response.Headers[customHeader.Key] = customHeader.Value;

				return Task.CompletedTask;
			});

			await this.next(httpContext);
		}
	}
}
