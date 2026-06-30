using System.Text.Json.Serialization;

namespace PoiskKinoMetadataPlugin.Models;

/// <summary>
/// Video information from PoiskKino API.
/// </summary>
public class PoiskKinoVideo
{
    /// <summary>
    /// Gets or sets the video URL.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets the video site.
    /// </summary>
    [JsonPropertyName("site")]
    public string? Site { get; set; }

    /// <summary>
    /// Gets or sets the video name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the video type.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}


