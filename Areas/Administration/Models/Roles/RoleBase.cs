using System.ComponentModel.DataAnnotations;
using Twenty57.Stadium.WebApp.Spa.Template.Areas.Administration.Models.Roles.Validation;

namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Administration.Models.Roles
{
	public class RoleBase
	{
		[Required]
		[UniqueName]
		public string Name { get; set; }

		public string[] Pages { get; set; }
	}
}
