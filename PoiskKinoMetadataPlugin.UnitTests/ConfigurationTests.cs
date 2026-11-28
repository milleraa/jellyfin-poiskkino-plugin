using System.Xml.Serialization;

namespace PoiskKinoMetadataPlugin.UnitTests;

/// <summary>
/// Тесты свойств <see cref="PluginConfiguration"/>: дефолт базового URL и совместимость XML-схемы
/// (AC-1, AC-8, риск A8 фичи `eu-api-endpoint`), дефолт и хранение лимита поиска
/// (AC-1, AC-8 фичи `search-limit-setting`, ADR-0004).
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

    // AC-1: без сохранённой конфигурации лимит поиска — дефолт 3 (историческое поведение).
    [Fact]
    public void SearchLimit_Default_IsThree()
    {
        var configuration = new PluginConfiguration();

        Assert.Equal(3, configuration.SearchLimit);
    }

    // AC-1: старая XML-конфигурация без <SearchLimit> даёт дефолт 3, прочие значения не сбрасываются.
    [Fact]
    public void Deserialize_LegacyXmlWithoutSearchLimit_KeepsDefaultAndOtherValues()
    {
        const string legacyXml =
            "<PluginConfiguration><ApiKey>legacy-key</ApiKey><ApiBaseUrl>https://eu-api.poiskkino.dev</ApiBaseUrl><IgnoreTmdbImages>false</IgnoreTmdbImages></PluginConfiguration>";
        var serializer = new XmlSerializer(typeof(PluginConfiguration));

        using var reader = new StringReader(legacyXml);
        var configuration = (PluginConfiguration)serializer.Deserialize(reader)!;

        Assert.Equal(3, configuration.SearchLimit);
        Assert.Equal("legacy-key", configuration.ApiKey);
        Assert.Equal("https://eu-api.poiskkino.dev", configuration.ApiBaseUrl);
        Assert.False(configuration.IgnoreTmdbImages);
    }

    // AC-8: значение в допустимом диапазоне проходит цикл сохранения и загрузки без изменений.
    [Fact]
    public void SerializeAndDeserialize_WithCustomSearchLimit_PreservesValue()
    {
        var serializer = new XmlSerializer(typeof(PluginConfiguration));

        using var writer = new StringWriter();
        serializer.Serialize(writer, new PluginConfiguration { ApiKey = "k", SearchLimit = 7 });

        using var reader = new StringReader(writer.ToString());
        var restored = (PluginConfiguration)serializer.Deserialize(reader)!;

        Assert.Equal(7, restored.SearchLimit);
        Assert.Equal("k", restored.ApiKey);
    }
}
