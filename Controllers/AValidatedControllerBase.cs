using Microsoft.AspNetCore.Mvc;

namespace Twenty57.Stadium.WebApp.Spa.Template.Controllers
{
	public abstract class AValidatedControllerBase : ControllerBase
	{
		protected IActionResult GetValidationResult(string message)
		{
			return ValidationProblem(new ValidationProblemDetails
			{
				Detail = message
			});
		}
	}
}
