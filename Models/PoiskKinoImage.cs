using System.Text.Json.Serialization;

namespace PoiskKinoMetadataPlugin.Models;

/// <summary>
/// Image information from PoiskKino API.
/// </summary>
public class PoiskKinoImage
{
    /// <summary>
    /// Gets or sets the image URL.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets the preview URL.
    /// </summary>
    [JsonPropertyName("previewUrl")]
    public string? PreviewUrl { get; set; }
}


