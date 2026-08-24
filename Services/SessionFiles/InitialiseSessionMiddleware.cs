using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.SessionFiles
{
	public class InitialiseSessionMiddleware
	{
		private readonly RequestDelegate next;

		public InitialiseSessionMiddleware(RequestDelegate next)
		{
			this.next = next;
		}

		public async Task InvokeAsync(HttpContext httpContext)
		{
			// The session ID changes for every request while the session is empty.
			// Populating the session with a dummy value ensures that concurrent requests
			// for a particular user will use the same session ID.
			const string key = "PersistSessionId";
			const string value = "dummy value";

			string dummyValue = httpContext.Session.GetString(key);
			if (dummyValue == null)
				httpContext.Session.SetString(key, value);

			await this.next(httpContext);
		}
	}
}
