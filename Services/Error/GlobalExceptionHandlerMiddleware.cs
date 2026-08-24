using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using Twenty57.Stadium.Core.Extensions;

namespace Twenty57.Stadium.WebApp.Spa.Template.Services.Error
{
	public class GlobalExceptionHandlerMiddleware
	{
		private readonly RequestDelegate next;
		private readonly ILogger<GlobalExceptionHandlerMiddleware> logger;

		public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
		{
			this.next = next;
			this.logger = logger;
		}

		public async Task InvokeAsync(HttpContext httpContext)
		{
			try
			{
				await this.next(httpContext);
			}
			catch (Exception exception)
			{
				string errorMessage = exception.GetDisplayMessage();
				this.logger.LogError(exception, errorMessage);

				if (httpContext.Response.HasStarted)
				{
					this.logger.LogWarning("The response has already started, the global exception handler will not be executed.");
					throw;
				}
				else
				{
					httpContext.Response.Clear();
					httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
					httpContext.Response.ContentType = "application/json";

					var problemDetails = new ProblemDetails
					{
						Status = (int)HttpStatusCode.InternalServerError,
						Instance = httpContext.Request.Path,
						Title = exception.Message,
						Detail = exception.InnerException?.GetDisplayMessage(": ")
					};

					await httpContext.Response.WriteAsJsonAsync(problemDetails);
				}
			}
		}
	}
}
