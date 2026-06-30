using System.Text.Json.Serialization;

namespace PoiskKinoMetadataPlugin.Models;

public class PoiskKinoName
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
