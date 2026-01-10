using System.Text.Json.Serialization;

namespace PoiskKinoMetadataPlugin.Models;

/// <summary>
/// Person information from PoiskKino API.
/// </summary>
public class PoiskKinoPerson
{
    /// <summary>
    /// Gets or sets the person ID.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the person name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the person English name.
    /// </summary>
    [JsonPropertyName("enName")]
    public string? EnName { get; set; }

    /// <summary>
    /// Gets or sets the person photo URL.
    /// </summary>
    [JsonPropertyName("photo")]
    public string? Photo { get; set; }

    /// <summary>
    /// Gets or sets the person description.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the person profession.
    /// </summary>
    [JsonPropertyName("profession")]
    public string? Profession { get; set; }

    /// <summary>
    /// Gets or sets the person English profession.
    /// </summary>
    [JsonPropertyName("enProfession")]
    public string? EnProfession { get; set; }
}


