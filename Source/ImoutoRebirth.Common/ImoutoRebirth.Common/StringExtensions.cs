namespace ImoutoRebirth.Common;

public static class StringExtensions
{
    public static string? TrimToNull(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input?.Trim()))
            return null;

        return input;
    }

    public static bool EqualsIgnoreCase(this string? str1, string? str2)
        => string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
}
