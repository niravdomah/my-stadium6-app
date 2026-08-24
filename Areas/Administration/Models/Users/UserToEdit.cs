using System.ComponentModel.DataAnnotations;
using Twenty57.Stadium.WebApp.Spa.Template.Areas.Administration.Models.Users.Validation;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Administration.Models.Users
{
	public class UserToEdit : UserBase, IIsAdministratorToBeEdited
	{
		[Required]
		[Exists]
		[IsNotLastAdministrator]
		public string Id { get; set; }
	}
}
