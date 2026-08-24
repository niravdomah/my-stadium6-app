using System.ComponentModel.DataAnnotations;
using Twenty57.Stadium.WebApp.Spa.Template.Areas.Administration.Models.Roles.Validation;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Administration.Models.Roles
{
	public class RoleToEdit : RoleBase
	{
		[Required]
		[Exists]
		public string Id { get; set; }
	}
}
