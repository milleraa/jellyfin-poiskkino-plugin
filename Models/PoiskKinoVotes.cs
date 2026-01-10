using System.Text.Json.Serialization;

namespace PoiskKinoMetadataPlugin.Models;

/// <summary>
/// Votes information from PoiskKino API.
/// </summary>
public class PoiskKinoVotes
{
    /// <summary>
    /// Gets or sets the Kinopoisk votes count.
    /// </summary>
    [JsonPropertyName("kp")]
    public int? Kp { get; set; }

    /// <summary>
    /// Gets or sets the IMDb votes count.
    /// </summary>
    [JsonPropertyName("imdb")]
    public int? Imdb { get; set; }

    /// <summary>
    /// Gets or sets the TMDB votes count.
    /// </summary>
    [JsonPropertyName("tmdb")]
    public int? Tmdb { get; set; }

    /// <summary>
    /// Gets or sets the film critics votes count.
    /// </summary>
    [JsonPropertyName("filmCritics")]
    public int? FilmCritics { get; set; }

    /// <summary>
    /// Gets or sets the Russian film critics votes count.
    /// </summary>
    [JsonPropertyName("russianFilmCritics")]
    public int? RussianFilmCritics { get; set; }

    /// <summary>
    /// Gets or sets the await votes count.
    /// </summary>
    [JsonPropertyName("await")]
    public int? Await { get; set; }
}


