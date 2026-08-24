using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using Twenty57.Stadium.WebApp.Spa.Administration;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.WebApi
{
	public class UserApiKeyValidationAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			var administrationContext = context.HttpContext.RequestServices.GetService<AdministrationContext>();
			string userApiKey = administrationContext.Application.WebApiKey;

			string receivedWebApiKey = context.ActionArguments["key"]?.ToString();

			if (receivedWebApiKey == null || receivedWebApiKey != userApiKey)
				throw new Exception("Invalid key");

			base.OnActionExecuting(context);
		}
	}
}
