using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Logic.Validation
{
    public class IdNumberAttribute : ValidationAttribute
    {
        private static readonly Regex ThirteenDigits = new(@"^\d{13}$", RegexOptions.Compiled);

        public IdNumberAttribute()
        {
            ErrorMessage = "ID number must be exactly 13 digits.";
        }

        public override bool IsValid(object? value)
        {
            // Presence is enforced separately by [Required] where applicable —
            // an empty value here is left for that attribute to reject, so
            // this only checks the format of whatever value is present.
            var idNumber = value as string;
            if (string.IsNullOrEmpty(idNumber)) return true;

            return ThirteenDigits.IsMatch(idNumber);
        }
    }
}
