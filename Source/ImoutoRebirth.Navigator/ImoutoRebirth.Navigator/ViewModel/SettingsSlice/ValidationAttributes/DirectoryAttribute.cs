using System.ComponentModel.DataAnnotations;
using System.IO;

namespace ImoutoRebirth.Navigator.ViewModel.SettingsSlice.ValidationAttributes;

public class DirectoryAttribute : ValidationAttribute
{
    private readonly string? _customMessage;

    public DirectoryAttribute(string? customMessage = null) => _customMessage = customMessage;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var name = validationContext.DisplayName;
        string[] members = validationContext.MemberName == null ? [] : [validationContext.MemberName];
        
        if (value is not string str)
            throw new InvalidOperationException($"{name} should be string");

        try
        {
            _ = new DirectoryInfo(str);
        }
        catch (Exception)
        {
            return new ValidationResult(_customMessage ?? $"{name} is in incorrect format", members);
        }
        
        return ValidationResult.Success;
    }
}
