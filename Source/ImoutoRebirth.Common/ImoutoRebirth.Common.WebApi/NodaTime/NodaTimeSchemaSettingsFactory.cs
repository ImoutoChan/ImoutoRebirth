using System.Text.Json;

namespace ImoutoRebirth.Common.WebApi.NodaTime;

public static class NodaTimeSchemaSettingsFactory
{
    public static NodaTimeSchemaSettings CreateNodaTimeSchemaSettings(
        this JsonSerializerOptions jsonSerializerOptions)
    {
        return new NodaTimeSchemaSettings(ResolvePropertyName, FormatToJson, true);

        string ResolvePropertyName(string propertyName)
            => jsonSerializerOptions.PropertyNamingPolicy?.ConvertName(propertyName) ?? propertyName;

        string FormatToJson(object value)
            => JsonSerializer.Serialize(value, jsonSerializerOptions).Trim('\"');
    }
}
