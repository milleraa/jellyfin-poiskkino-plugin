using System.Text.Json.Serialization;

namespace PoiskKinoMetadataPlugin.Models;

/// <summary>
/// Genre information from PoiskKino API.
/// </summary>
public class PoiskKinoGenre
{
    /// <summary>
    /// Gets or sets the genre name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}


