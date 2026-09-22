using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Logic.Validation
{
    public class PhoneNumberDigitsAttribute : ValidationAttribute
    {
        private static readonly Regex TenDigits = new(@"^\d{10}$", RegexOptions.Compiled);

        public PhoneNumberDigitsAttribute()
        {
            ErrorMessage = "Phone number must be exactly 10 digits.";
        }

        public override bool IsValid(object? value)
        {
            // Presence is enforced separately by [Required] where applicable —
            // an empty value here is left for that attribute to reject, so
            // this only checks the format of whatever value is present.
            var phoneNumber = value as string;
            if (string.IsNullOrEmpty(phoneNumber)) return true;

            return TenDigits.IsMatch(phoneNumber);
        }
    }
}
