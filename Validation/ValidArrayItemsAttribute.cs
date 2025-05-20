using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace HarmonicArchiveBackend.Validation
{
    public class ValidArrayItemsAttribute : ValidationAttribute
    {
        private readonly string _pattern;

        public ValidArrayItemsAttribute(string pattern)
        {
            _pattern = pattern;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is List<string> items)
            {
                var regex = new Regex(_pattern);
                foreach (var item in items)
                {
                    if (!regex.IsMatch(item))
                    {
                        return new ValidationResult($"Invalid value in {validationContext.DisplayName}.");
                    }
                }
            }

            return ValidationResult.Success;
        }
    }
}