using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Twenty57.Stadium.WebApp.Spa.Template.Models.Validation
{
	public class EmailAttribute : DataTypeAttribute
	{
		// language=regex
		public const string ValidEmailPattern = @"^[a-zA-Z0-9.!#$%&'*+\/=?^_`{|}~-]{1,64}@[a-zA-Z0-9-]{1,64}\.[\.a-zA-Z0-9-]{2,64}$";

		public EmailAttribute()
			: base(DataType.EmailAddress)
		{ }

		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			string email = (string)value;
			if (string.IsNullOrEmpty(email) || Regex.IsMatch(email, ValidEmailPattern))
				return ValidationResult.Success;

			return new ValidationResult("Invalid email");
		}
	}
}
