using System.Text.Json.Serialization;

namespace PoiskKinoMetadataPlugin.Models;

/// <summary>
/// External IDs from PoiskKino API.
/// </summary>
public class PoiskKinoExternalId
{
    /// <summary>
    /// Gets or sets the IMDb ID.
    /// </summary>
    [JsonPropertyName("imdb")]
    public string? Imdb { get; set; }

    /// <summary>
    /// Gets or sets the TMDB ID.
    /// </summary>
    [JsonPropertyName("tmdb")]
    public int? Tmdb { get; set; }

    /// <summary>
    /// Gets or sets the KinoPoisk HD ID.
    /// </summary>
    [JsonPropertyName("kpHD")]
    public string? KpHd { get; set; }
}


