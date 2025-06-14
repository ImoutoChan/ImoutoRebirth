using System.Text.Json.Serialization;

namespace ImoutoRebirth.Tori.UI.Models;

public class Configuration
{
    [JsonPropertyName("DanbooruLogin")]
    public string? DanbooruLogin { get; set; }

    [JsonPropertyName("DanbooruApiKey")]
    public string? DanbooruApiKey { get; set; }

    [JsonPropertyName("SankakuLogin")]
    public string? SankakuLogin { get; set; }

    [JsonPropertyName("SankakuPassword")]
    public string? SankakuPassword { get; set; }

    [JsonPropertyName("YandereLogin")]
    public string? YandereLogin { get; set; }

    [JsonPropertyName("YandereApiKey")]
    public string? YandereApiKey { get; set; }

    [JsonPropertyName("RoomPort")]
    public string RoomPort { get; set; } = "11301";

    [JsonPropertyName("LilinPort")]
    public string LilinPort { get; set; } = "11302";

    [JsonPropertyName("KekkaiPort")]
    public string KekkaiPort { get; set; } = "11303";

    [JsonPropertyName("HarpySavePath")]
    public string? HarpySavePath { get; set; }

    [JsonPropertyName("HarpyFavoritesSaveJobRepeatEveryMinutes")]
    public string HarpyFavoritesSaveJobRepeatEveryMinutes { get; set; } = "1";

    [JsonPropertyName("KekkaiAuthToken")]
    public string KekkaiAuthToken { get; set; } = "";

    [JsonPropertyName("LilinConnectionString")]
    public string LilinConnectionString { get; set; } = "Server=localhost;Port=5432;Database=LilinProd;User Id=postgres;Password=postgres;";

    [JsonPropertyName("MeidoConnectionString")]
    public string MeidoConnectionString { get; set; } = "Server=localhost;Port=5432;Database=MeidoProd;User Id=postgres;Password=postgres;";

    [JsonPropertyName("RoomConnectionString")]
    public string RoomConnectionString { get; set; } = "Server=localhost;Port=5432;Database=RoomProd;User Id=postgres;Password=postgres;";

    [JsonPropertyName("MassTransitConnectionString")]
    public string MassTransitConnectionString { get; set; } = "Server=localhost;Port=5432;Database=masstransit;User Id=postgres;Password=postgres;";

    [JsonPropertyName("MeidoMetadataActualizerRepeatEveryMinutes")]
    public string MeidoMetadataActualizerRepeatEveryMinutes { get; set; } = "5";

    [JsonPropertyName("MeidoFaultToleranceRepeatEveryMinutes")]
    public string MeidoFaultToleranceRepeatEveryMinutes { get; set; } = "10080";

    [JsonPropertyName("MeidoFaultToleranceIsEnabled")]
    public string MeidoFaultToleranceIsEnabled { get; set; } = "true";

    [JsonPropertyName("RoomImoutoPicsUploadUrl")]
    public string? RoomImoutoPicsUploadUrl { get; set; }

    [JsonPropertyName("InstallLocation")]
    public string InstallLocation { get; set; } = "C:\\Program Files\\Imouto";

    [JsonPropertyName("OpenSearchUri")]
    public string? OpenSearchUri { get; set; } = "http://localhost:9200/";
    
    [JsonPropertyName("JaegerHost")]
    public string JaegerHost { get; set; } = "localhost";
    
    [JsonPropertyName("JaegerPort")]
    public string JaegerPort { get; set; } = "6831";
    
    [JsonPropertyName("ExHentaiIpbMemberId")]
    public string? ExHentaiIpbMemberId { get; set; }
    
    [JsonPropertyName("ExHentaiIpbPassHash")]
    public string? ExHentaiIpbPassHash { get; set; }
    
    [JsonPropertyName("ExHentaiIgneous")]
    public string? ExHentaiIgneous { get; set; }
    
    [JsonPropertyName("ExHentaiUserAgent")]
    public string? ExHentaiUserAgent { get; set; }
} 