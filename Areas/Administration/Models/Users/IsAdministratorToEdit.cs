using System.ComponentModel.DataAnnotations;
using Twenty57.Stadium.WebApp.Spa.Template.Areas.Administration.Models.Users.Validation;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Administration.Models.Users
{
	public class IsAdministratorToEdit : IIsAdministratorToBeEdited
	{
		[Required]
		[Exists]
		[IsNotLoggedIn(ErrorMessage = "The currently logged-in user cannot be removed as administrator")]
		[IsNotLastAdministrator]
		public string Id { get; set; }

		public bool IsAdministrator { get; set; }
	}
}
