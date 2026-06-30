using System.Text.Json.Serialization;

namespace PoiskKinoMetadataPlugin.Models;

/// <summary>
/// Response model from PoiskKino API v1.4 season endpoint (SeasonDocsResponseDtoV1_4).
/// </summary>
public class PoiskKinoSeasonResponse
{
    /// <summary>
    /// Gets or sets the season results (docs).
    /// </summary>
    [JsonPropertyName("docs")]
    public List<PoiskKinoSeason>? Docs { get; set; }

    /// <summary>
    /// Gets or sets the total number of results.
    /// </summary>
    [JsonPropertyName("total")]
    public int Total { get; set; }

    /// <summary>
    /// Gets or sets the limit per page.
    /// </summary>
    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    /// <summary>
    /// Gets or sets the current page number.
    /// </summary>
    [JsonPropertyName("page")]
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets the total number of pages.
    /// </summary>
    [JsonPropertyName("pages")]
    public int Pages { get; set; }
}


