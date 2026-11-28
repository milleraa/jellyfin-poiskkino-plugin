using System.Xml.Serialization;

namespace PoiskKinoMetadataPlugin.UnitTests;

/// <summary>
/// Тесты свойств <see cref="PluginConfiguration"/>: дефолт базового URL и совместимость XML-схемы
/// (AC-1, AC-8, риск A8 фичи `eu-api-endpoint`).
/// </summary>
public class PluginConfigurationTests
{
    [Fact]
    public void ApiBaseUrl_Default_EqualsResolverDefault()
    {
        var configuration = new PluginConfiguration();

        Assert.Equal(ApiBaseUrlResolver.DefaultBaseUrl, configuration.ApiBaseUrl);
        Assert.Equal("https://api.poiskkino.dev", configuration.ApiBaseUrl);
    }

    // AC-8: старая XML-конфигурация без <ApiBaseUrl> оставляет инициализатор, прочие значения читаются.
    [Fact]
    public void Deserialize_LegacyXmlWithoutApiBaseUrl_KeepsDefaultAndOtherValues()
    {
        const string legacyXml =
            "<PluginConfiguration><ApiKey>legacy-key</ApiKey><IgnoreTmdbImages>false</IgnoreTmdbImages></PluginConfiguration>";
        var serializer = new XmlSerializer(typeof(PluginConfiguration));

        using var reader = new StringReader(legacyXml);
        var configuration = (PluginConfiguration)serializer.Deserialize(reader)!;

        Assert.Equal("legacy-key", configuration.ApiKey);
        Assert.False(configuration.IgnoreTmdbImages);
        Assert.Equal(ApiBaseUrlResolver.DefaultBaseUrl, configuration.ApiBaseUrl);
    }

    [Fact]
    public void SerializeAndDeserialize_WithCustomApiBaseUrl_PreservesValueVerbatim()
    {
        // D-ARCH-4: канонизация не выполняется — в конфигурации остаётся ввод администратора.
        const string rawApiBaseUrl = "https://eu-api.poiskkino.dev/";
        var serializer = new XmlSerializer(typeof(PluginConfiguration));

        using var writer = new StringWriter();
        serializer.Serialize(writer, new PluginConfiguration { ApiKey = "k", ApiBaseUrl = rawApiBaseUrl });

        using var reader = new StringReader(writer.ToString());
        var restored = (PluginConfiguration)serializer.Deserialize(reader)!;

        Assert.Equal(rawApiBaseUrl, restored.ApiBaseUrl);
        Assert.Equal("k", restored.ApiKey);
    }
}
