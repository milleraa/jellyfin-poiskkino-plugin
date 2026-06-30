using System.Text.Json.Serialization;

namespace PoiskKinoMetadataPlugin.Models;

public class PoiskKinoSeasonCursorResponse
{
    [JsonPropertyName("docs")]
    public List<PoiskKinoSeason>? Docs { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("next")]
    public string? Next { get; set; }

    [JsonPropertyName("prev")]
    public string? Prev { get; set; }

    [JsonPropertyName("hasNext")]
    public bool HasNext { get; set; }

    [JsonPropertyName("hasPrev")]
    public bool HasPrev { get; set; }

    [JsonPropertyName("total")]
    public int? Total { get; set; }
}
