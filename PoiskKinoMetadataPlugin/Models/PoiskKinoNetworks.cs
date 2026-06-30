using System.Text.Json.Serialization;

namespace PoiskKinoMetadataPlugin.Models;

public class PoiskKinoNetworks
{
    [JsonPropertyName("items")]
    public List<PoiskKinoNetworkItem>? Items { get; set; }
}
