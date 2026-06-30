using System.Text.Json.Serialization;

namespace PoiskKinoMetadataPlugin.Models;

public class PoiskKinoReviewInfo
{
    [JsonPropertyName("count")]
    public int? Count { get; set; }

    [JsonPropertyName("positiveCount")]
    public int? PositiveCount { get; set; }

    [JsonPropertyName("percentage")]
    public string? Percentage { get; set; }
}
