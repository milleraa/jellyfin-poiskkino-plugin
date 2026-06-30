using System.Text.Json.Serialization;

namespace PoiskKinoMetadataPlugin.Models;

public class PoiskKinoPremiere
{
    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("world")]
    public string? World { get; set; }

    [JsonPropertyName("russia")]
    public string? Russia { get; set; }

    [JsonPropertyName("digital")]
    public string? Digital { get; set; }

    [JsonPropertyName("cinema")]
    public string? Cinema { get; set; }

    [JsonPropertyName("bluray")]
    public string? Bluray { get; set; }

    [JsonPropertyName("dvd")]
    public string? Dvd { get; set; }
}
