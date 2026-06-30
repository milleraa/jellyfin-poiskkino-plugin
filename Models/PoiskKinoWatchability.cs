using System.Text.Json.Serialization;

namespace PoiskKinoMetadataPlugin.Models;

public class PoiskKinoWatchability
{
    [JsonPropertyName("items")]
    public List<PoiskKinoWatchabilityItem>? Items { get; set; }
}

public class PoiskKinoWatchabilityItem
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("logo")]
    public PoiskKinoImage? Logo { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
