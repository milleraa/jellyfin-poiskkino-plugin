using System.Text.Json.Serialization;

namespace PoiskKinoMetadataPlugin.Models;

/// <summary>
/// Country information from PoiskKino API.
/// </summary>
public class PoiskKinoCountry
{
    /// <summary>
    /// Gets or sets the country name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}


