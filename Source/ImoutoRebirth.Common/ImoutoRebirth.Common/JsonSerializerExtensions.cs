using System.Text.Json;

namespace ImoutoRebirth.Common;

public static class DefaultJsonSerializer
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string SerializeIndented<T>(T value)
        => JsonSerializer.Serialize(value, Options);
}
