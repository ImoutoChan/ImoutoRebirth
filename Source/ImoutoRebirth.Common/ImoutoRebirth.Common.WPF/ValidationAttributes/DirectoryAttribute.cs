using System.ComponentModel.DataAnnotations;
using System.IO;

namespace ImoutoRebirth.Common.WPF.ValidationAttributes;

public class DirectoryAttribute : ValidationAttribute
{
    private readonly string? _customMessage;
    private readonly bool _onlyAbsolutePath;

    public DirectoryAttribute(string? customMessage = null, bool onlyAbsolutePath = false)
    {
        _customMessage = customMessage;
        _onlyAbsolutePath = onlyAbsolutePath;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        var name = validationContext.DisplayName;
        string[] members = validationContext.MemberName == null ? [] : [validationContext.MemberName];

        if (value is not string str)
            throw new InvalidOperationException($"{name} should be string");

        if (str == string.Empty)
            return ValidationResult.Success;

        try
        {
            _ = new DirectoryInfo(str);

            return _onlyAbsolutePath == true && !Path.IsPathRooted(str)
                ? new ValidationResult(_customMessage ?? $"{name} should be absolute path", members)
                : ValidationResult.Success;
        }
        catch (Exception)
        {
            return new ValidationResult(_customMessage ?? $"{name} is in incorrect format", members);
        }
    }
}
