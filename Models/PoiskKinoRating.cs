using System.Text.Json.Serialization;

namespace PoiskKinoMetadataPlugin.Models;

/// <summary>
/// Rating information from PoiskKino API.
/// </summary>
public class PoiskKinoRating
{
    /// <summary>
    /// Gets or sets the Kinopoisk rating (0-10 scale).
    /// </summary>
    [JsonPropertyName("kinopoisk")]
    public double? Kinopoisk { get; set; }

    /// <summary>
    /// Gets or sets the IMDb rating (0-10 scale).
    /// </summary>
    [JsonPropertyName("imdb")]
    public double? Imdb { get; set; }

    /// <summary>
    /// Gets or sets the TMDB rating.
    /// </summary>
    [JsonPropertyName("tmdb")]
    public double? Tmdb { get; set; }

    /// <summary>
    /// Gets or sets the film critics rating.
    /// </summary>
    [JsonPropertyName("filmCritics")]
    public double? FilmCritics { get; set; }

    /// <summary>
    /// Gets or sets the Russian film critics rating.
    /// </summary>
    [JsonPropertyName("russianFilmCritics")]
    public double? RussianFilmCritics { get; set; }

    /// <summary>
    /// Gets or sets the await rating.
    /// </summary>
    [JsonPropertyName("await")]
    public double? Await { get; set; }
}


