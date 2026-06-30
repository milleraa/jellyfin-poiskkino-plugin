using System.Text.Json.Serialization;

namespace PoiskKinoMetadataPlugin.Models;

/// <summary>
/// Episode information from PoiskKino API v1.4 (EpisodeV1_4).
/// </summary>
public class PoiskKinoEpisode
{
    /// <summary>
    /// Gets or sets the episode number.
    /// </summary>
    [JsonPropertyName("number")]
    public int Number { get; set; }

    /// <summary>
    /// Gets or sets the episode name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the episode English name.
    /// </summary>
    [JsonPropertyName("enName")]
    public string? EnName { get; set; }

    /// <summary>
    /// Gets or sets the episode air date (ISO format string).
    /// </summary>
    [JsonPropertyName("airDate")]
    public string? AirDate { get; set; }

    /// <summary>
    /// Gets or sets the episode description.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the episode English description.
    /// </summary>
    [JsonPropertyName("enDescription")]
    public string? EnDescription { get; set; }

    /// <summary>
    /// Gets or sets the episode still image.
    /// </summary>
    [JsonPropertyName("still")]
    public PoiskKinoImage? Still { get; set; }

    /// <summary>
    /// Gets or sets the episode date (deprecated field from API, use AirDate instead).
    /// </summary>
    [JsonPropertyName("date")]
    public string? ApiDate { get; set; }

    /// <summary>
    /// Gets or sets the episode date parsed as DateTime (prefer AirDate over this).
    /// </summary>
    [JsonIgnore]
    public DateTime? Date => !string.IsNullOrEmpty(AirDate) && DateTime.TryParse(AirDate, out var date) 
        ? date 
        : !string.IsNullOrEmpty(ApiDate) && DateTime.TryParse(ApiDate, out var apiDate) 
            ? apiDate 
            : null;
}


