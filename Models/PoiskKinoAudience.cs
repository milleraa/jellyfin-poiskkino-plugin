using System.Text.Json.Serialization;

namespace PoiskKinoMetadataPlugin.Models;

public class PoiskKinoAudience
{
    [JsonPropertyName("count")]
    public double? Count { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }
}
