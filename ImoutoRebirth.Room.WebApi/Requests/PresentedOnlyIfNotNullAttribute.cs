using System.ComponentModel.DataAnnotations;

namespace ImoutoRebirth.Room.WebApi.Requests
{
    /// <summary>
    /// Validate that others required value is presented when current field is not null.
    /// </summary>
    public class PresentedOnlyIfNotNullAttribute : ValidationAttribute
    {
        private readonly string[] _requiredValueNames;

        public PresentedOnlyIfNotNullAttribute(params string[] requiredValueNames)
        {
            _requiredValueNames = requiredValueNames;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)                 
        {
            if (value == null)
                return ValidationResult.Success;

            var values = _requiredValueNames
                .Select(x
                    => validationContext.Items.FirstOrDefault(y => y.Key is string key && key == x))
                .Where(x => x.Key != null)
                .Select(x => x.Value)
                .ToList();

            var anyNull = values.Any(x => x == null);

            return anyNull 
                ? new ValidationResult("Value is presented even though required values has nulls.") 
                : ValidationResult.Success;
        }
    }
}