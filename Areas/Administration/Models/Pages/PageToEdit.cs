using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Twenty57.Stadium.WebApp.Spa.Template.Areas.Administration.Models.Pages.Validation;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Administration.Models.Pages
{
	public class PageToEdit
	{
		[Required]
		[Exists]
		public string Id { get; set; }

		public IEnumerable<string> Roles { get; set; }
	}
}
