using System.Text.Json.Serialization;

namespace PoiskKinoMetadataPlugin.Models;

/// <summary>
/// Videos collection from PoiskKino API.
/// </summary>
public class PoiskKinoVideos
{
    /// <summary>
    /// Gets or sets the trailers list.
    /// </summary>
    [JsonPropertyName("trailers")]
    public List<PoiskKinoVideo>? Trailers { get; set; }
}


