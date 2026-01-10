using System.Text.Json.Serialization;

namespace PoiskKinoMetadataPlugin.Models;

/// <summary>
/// Season data from PoiskKino API v1.4 (SeasonV1_4).
/// </summary>
public class PoiskKinoSeason
{
    /// <summary>
    /// Gets or sets the movie ID.
    /// </summary>
    [JsonPropertyName("movieId")]
    public int MovieId { get; set; }

    /// <summary>
    /// Gets or sets the season number.
    /// </summary>
    [JsonPropertyName("number")]
    public int Number { get; set; }

    /// <summary>
    /// Gets or sets the episodes count.
    /// </summary>
    [JsonPropertyName("episodesCount")]
    public int? EpisodesCount { get; set; }

    /// <summary>
    /// Gets or sets the episodes list.
    /// </summary>
    [JsonPropertyName("episodes")]
    public List<PoiskKinoEpisode>? Episodes { get; set; }

    /// <summary>
    /// Gets or sets the season poster.
    /// </summary>
    [JsonPropertyName("poster")]
    public PoiskKinoImage? Poster { get; set; }

    /// <summary>
    /// Gets or sets the season name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the season English name.
    /// </summary>
    [JsonPropertyName("enName")]
    public string? EnName { get; set; }

    /// <summary>
    /// Gets or sets the season duration in minutes.
    /// </summary>
    [JsonPropertyName("duration")]
    public int? Duration { get; set; }

    /// <summary>
    /// Gets or sets the season description.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the season English description.
    /// </summary>
    [JsonPropertyName("enDescription")]
    public string? EnDescription { get; set; }

    /// <summary>
    /// Gets or sets the season air date (ISO format string).
    /// </summary>
    [JsonPropertyName("airDate")]
    public string? AirDate { get; set; }
}


