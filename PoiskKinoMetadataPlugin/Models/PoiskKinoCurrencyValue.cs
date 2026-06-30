using System.Text.Json.Serialization;

namespace PoiskKinoMetadataPlugin.Models;

public class PoiskKinoCurrencyValue
{
    [JsonPropertyName("value")]
    public double? Value { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }
}
