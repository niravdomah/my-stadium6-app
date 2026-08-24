namespace Twenty57.Stadium.WebApp.Spa.Template.Areas.Administration.Models.Users
{
	public interface IIsAdministratorToBeEdited
	{
		string Id { get; set; }

		bool IsAdministrator { get; set; }
	}
}
