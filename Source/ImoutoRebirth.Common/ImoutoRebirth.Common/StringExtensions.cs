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

    public static bool ContainsIgnoreCase(this string str1, string str2)
        => str1.Contains(str2, StringComparison.OrdinalIgnoreCase);

    public static bool ContainsOrContainedIn(this string str1, string str2)
        => str1.Contains(str2) || str2.Contains(str1);

    public static bool EndsWithIgnoreCase(this string? str1, string str2)
        => str1?.EndsWith(str2, StringComparison.OrdinalIgnoreCase) ?? false;

    public static bool StartsWithIgnoreCase(this string? str1, string str2)
        => str1?.StartsWith(str2, StringComparison.OrdinalIgnoreCase) ?? false;

    public static int? GetIntOrDefault(this string? str)
    {
        if (int.TryParse(str, out var result))
            return result;

        return null;
    }

    public static int GetInt(this string? str)
    {
        if (int.TryParse(str, out var result))
            return result;

        throw new InvalidOperationException("Cannot parse string to int");
    }
}
