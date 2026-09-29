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
            var idNumber = value as string;
            if (string.IsNullOrEmpty(idNumber)) return true;

            return ThirteenDigits.IsMatch(idNumber);
        }
    }
}
