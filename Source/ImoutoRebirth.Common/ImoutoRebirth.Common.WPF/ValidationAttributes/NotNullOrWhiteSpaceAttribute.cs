using System.ComponentModel.DataAnnotations;

namespace ImoutoRebirth.Common.WPF.ValidationAttributes;

public class NotNullOrWhiteSpaceAttribute : ValidationAttribute
{
    private readonly string? _customMessage;

    public NotNullOrWhiteSpaceAttribute(string? customMessage = null) => _customMessage = customMessage;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var name = validationContext?.DisplayName;
        string[] members = validationContext?.MemberName == null ? [] : [validationContext.MemberName];
        
        if (value is not string str || string.IsNullOrWhiteSpace(str))
        {
            return new ValidationResult(_customMessage ?? $"{name} should not be empty", members);
        }

        return ValidationResult.Success;
    }
}
