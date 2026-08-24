using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Twenty57.Stadium.Server.Shared;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.WebApi
{
	public class WebApiKeyValidationAttribute : ActionFilterAttribute
	{
		public const string ApiKeyConfigurationName = "WebApiKey";

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			string webApiKey = GetConfiguredApiKey(context);
			string receivedWebApiKey = context.ActionArguments["key"]?.ToString();

			if (receivedWebApiKey == null || receivedWebApiKey != webApiKey)
				throw new Exception("Invalid key");

			base.OnActionExecuting(context);
		}

		private static string GetConfiguredApiKey(ActionExecutingContext context)
		{
			if (OperatingSystem.IsWindows())
				return RegistryKeys.Instance.ApiKey;

			var configuration = context.HttpContext.RequestServices.GetService<IConfiguration>();
			return configuration?[ApiKeyConfigurationName];
		}
	}
}
