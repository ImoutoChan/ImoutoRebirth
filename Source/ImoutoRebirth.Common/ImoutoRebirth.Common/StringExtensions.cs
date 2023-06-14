namespace ImoutoRebirth.Common;

public static class StringExtensions
{
    public static string? TrimToNull(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input?.Trim()))
            return null;

        return input;
    }
}
