using System.Text.Json.Serialization;

namespace PoiskKinoMetadataPlugin.Models;

public class PoiskKinoFees
{
    [JsonPropertyName("world")]
    public PoiskKinoCurrencyValue? World { get; set; }

    [JsonPropertyName("russia")]
    public PoiskKinoCurrencyValue? Russia { get; set; }

    [JsonPropertyName("usa")]
    public PoiskKinoCurrencyValue? Usa { get; set; }
}
