using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System.Threading.Tasks;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.RequestLimits
{
	// Linux/Kestrel only: enforces the web.config upload-size limit that IIS enforces on Windows.
	public class MaxRequestBodySizeMiddleware
	{
		private readonly RequestDelegate next;
		private readonly WebConfigSettingsCache settingsCache;

		public MaxRequestBodySizeMiddleware(RequestDelegate next, WebConfigSettingsCache settingsCache)
		{
			this.next = next;
			this.settingsCache = settingsCache;
		}

		public async Task InvokeAsync(HttpContext httpContext)
		{
			var maxRequestBodySizeFeature = httpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();

			// The feature locks (IsReadOnly) once the body has started being read; this middleware runs early
			// enough that it is still writable.
			if (maxRequestBodySizeFeature != null && !maxRequestBodySizeFeature.IsReadOnly)
				maxRequestBodySizeFeature.MaxRequestBodySize = this.settingsCache.GetMaxAllowedContentLength();

			await this.next(httpContext);
		}
	}
}
