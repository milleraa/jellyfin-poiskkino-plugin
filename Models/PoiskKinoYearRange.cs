using System.Text.Json.Serialization;

namespace PoiskKinoMetadataPlugin.Models;

public class PoiskKinoYearRange
{
    [JsonPropertyName("start")]
    public int? Start { get; set; }

    [JsonPropertyName("end")]
    public int? End { get; set; }
}
