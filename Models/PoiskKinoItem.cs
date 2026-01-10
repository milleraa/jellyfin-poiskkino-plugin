using System.Text.Json.Serialization;

namespace PoiskKinoMetadataPlugin.Models;

/// <summary>
/// Single item from PoiskKino API v1.4 search (SearchMovieDtoV1_4).
/// </summary>
public class PoiskKinoItem
{
    /// <summary>
    /// Gets or sets the item ID.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name (title).
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the alternative name.
    /// </summary>
    [JsonPropertyName("alternativeName")]
    public string? AlternativeName { get; set; }

    /// <summary>
    /// Gets or sets the English name.
    /// </summary>
    [JsonPropertyName("enName")]
    public string? EnName { get; set; }

    /// <summary>
    /// Gets or sets the item type.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the release year.
    /// </summary>
    [JsonPropertyName("year")]
    public int? Year { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the short description.
    /// </summary>
    [JsonPropertyName("shortDescription")]
    public string? ShortDescription { get; set; }

    /// <summary>
    /// Gets or sets the movie length in minutes.
    /// </summary>
    [JsonPropertyName("movieLength")]
    public int? MovieLength { get; set; }

    /// <summary>
    /// Gets or sets the external IDs.
    /// </summary>
    [JsonPropertyName("externalId")]
    public PoiskKinoExternalId? ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the poster image.
    /// </summary>
    [JsonPropertyName("poster")]
    public PoiskKinoImage? Poster { get; set; }

    /// <summary>
    /// Gets or sets the backdrop image.
    /// </summary>
    [JsonPropertyName("backdrop")]
    public PoiskKinoImage? Backdrop { get; set; }

    /// <summary>
    /// Gets or sets the rating information.
    /// </summary>
    [JsonPropertyName("rating")]
    public PoiskKinoRating? Rating { get; set; }

    /// <summary>
    /// Gets or sets the votes information.
    /// </summary>
    [JsonPropertyName("votes")]
    public PoiskKinoVotes? Votes { get; set; }

    /// <summary>
    /// Gets or sets the genres list.
    /// </summary>
    [JsonPropertyName("genres")]
    public List<PoiskKinoGenre>? Genres { get; set; }

    /// <summary>
    /// Gets or sets the countries list.
    /// </summary>
    [JsonPropertyName("countries")]
    public List<PoiskKinoCountry>? Countries { get; set; }

    /// <summary>
    /// Gets or sets whether this is a series.
    /// </summary>
    [JsonPropertyName("isSeries")]
    public bool? IsSeries { get; set; }

    /// <summary>
    /// Gets or sets the series length in minutes.
    /// </summary>
    [JsonPropertyName("seriesLength")]
    public int? SeriesLength { get; set; }

    /// <summary>
    /// Gets or sets the total series length in minutes.
    /// </summary>
    [JsonPropertyName("totalSeriesLength")]
    public int? TotalSeriesLength { get; set; }

}


