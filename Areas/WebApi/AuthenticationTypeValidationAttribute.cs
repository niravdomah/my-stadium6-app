using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using Twenty57.Stadium.WebApp.Spa.Core.Enums;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Configuration;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.WebApi
{
	public class AuthenticationTypeValidationAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			var configOptions = context.HttpContext.RequestServices.GetService<IOptionsSnapshot<Config>>();
			var authenticationType = configOptions.Value.AuthenticationType;

			if (authenticationType == AuthenticationType.Anonymous)
				throw new Exception("Invalid security mode");

			base.OnActionExecuting(context);
		}
	}
}
