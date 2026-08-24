using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Twenty57.Stadium.WebApp.Spa.Administration;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Administration.Models.Pages.Validation
{
	public class ExistsAttribute : ValidationAttribute
	{
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			string pageId = (string)value;

			var pages = validationContext.GetService<AdministrationContext>().Pages;
			var page = pages.AsQueryable()
							.SingleOrDefault(p => p.Id == pageId);

			if (page != null)
				return ValidationResult.Success;

			return new ValidationResult("Page not found");
		}
	}
}
