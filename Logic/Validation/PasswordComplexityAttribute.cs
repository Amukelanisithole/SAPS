using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Logic.Validation
{
    public class PasswordComplexityAttribute : ValidationAttribute
    {
        private static readonly Regex HasUpper = new("[A-Z]", RegexOptions.Compiled);
        private static readonly Regex HasLower = new("[a-z]", RegexOptions.Compiled);
        private static readonly Regex HasDigit = new("[0-9]", RegexOptions.Compiled);
        private static readonly Regex HasSpecial = new(@"[^a-zA-Z0-9]", RegexOptions.Compiled);

        public PasswordComplexityAttribute()
        {
            ErrorMessage = "Password must be at least 8 characters and include an uppercase letter, a lowercase letter, a digit, and a special character.";
        }

        public override bool IsValid(object? value)
        {
            var password = value as string;
            if (string.IsNullOrEmpty(password)) return false;

            return password.Length >= 8
                && HasUpper.IsMatch(password)
                && HasLower.IsMatch(password)
                && HasDigit.IsMatch(password)
                && HasSpecial.IsMatch(password);
        }
    }
}
