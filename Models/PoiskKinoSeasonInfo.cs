using System.Text.Json.Serialization;

namespace PoiskKinoMetadataPlugin.Models;

/// <summary>
/// Season information from PoiskKino API.
/// </summary>
public class PoiskKinoSeasonInfo
{
    /// <summary>
    /// Gets or sets the season number.
    /// </summary>
    [JsonPropertyName("number")]
    public int? Number { get; set; }

    /// <summary>
    /// Gets or sets the episodes count.
    /// </summary>
    [JsonPropertyName("episodesCount")]
    public int? EpisodesCount { get; set; }
}


