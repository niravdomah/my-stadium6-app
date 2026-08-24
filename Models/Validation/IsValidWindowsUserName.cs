using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Twenty57.Stadium.WebApp.Spa.Core.Enums;
using Twenty57.Stadium.WebApp.Spa.Core.Services.Configuration;

namespace Twenty57.Stadium.WebApp.Spa.Template.Models.Validation
{
	public class IsValidWindowsUserName : ValidationAttribute
	{
		// language=regex
		public const string ValidWindowsUserNamePattern = @"^[a-zA-Z0-9][a-zA-Z0-9\-\.]{0,62}\\\w[\w\.\- ]+$";

		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			var authenticationType = AppSettingsHelper.GetInstance().AppSettings.Config.AuthenticationType;

			if (authenticationType != AuthenticationType.Windows)
				return ValidationResult.Success;

			string userName = (string)value;
			if (string.IsNullOrEmpty(userName))
				return new ValidationResult("[UserName] is required");

			if (Regex.IsMatch(userName, ValidWindowsUserNamePattern))
				return ValidationResult.Success;

			return new ValidationResult("Please enter a valid Windows Domain user name (domain\\user)");
		}
	}
}
