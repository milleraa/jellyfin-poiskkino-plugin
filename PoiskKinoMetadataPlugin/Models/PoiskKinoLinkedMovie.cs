using System.Text.Json.Serialization;

namespace PoiskKinoMetadataPlugin.Models;

public class PoiskKinoLinkedMovie
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("enName")]
    public string? EnName { get; set; }

    [JsonPropertyName("alternativeName")]
    public string? AlternativeName { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("poster")]
    public PoiskKinoImage? Poster { get; set; }

    [JsonPropertyName("rating")]
    public PoiskKinoRating? Rating { get; set; }

    [JsonPropertyName("year")]
    public int? Year { get; set; }
}
