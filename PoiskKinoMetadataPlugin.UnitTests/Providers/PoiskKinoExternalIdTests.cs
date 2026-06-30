using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace PoiskKinoMetadataPlugin.UnitTests.Providers;

public class PoiskKinoExternalIdTests
{
    private readonly PoiskKinoExternalId _externalId = new();

    [Fact]
    public void Supports_Series_ReturnsTrue()
    {
        var result = _externalId.Supports(new Series());

        Assert.True(result);
    }

    [Fact]
    public void Supports_Movie_ReturnsTrue()
    {
        var result = _externalId.Supports(new Movie());

        Assert.True(result);
    }

    [Fact]
    public void Supports_Episode_ReturnsFalse()
    {
        var result = _externalId.Supports(new Episode());

        Assert.False(result);
    }

    [Fact]
    public void Supports_Season_ReturnsFalse()
    {
        var result = _externalId.Supports(new Season());

        Assert.False(result);
    }

    [Fact]
    public void ProviderName_ReturnsCorrect()
    {
        Assert.Equal("ПоискКино", _externalId.ProviderName);
    }

    [Fact]
    public void Key_ReturnsCorrect()
    {
        Assert.Equal("PoiskKino", _externalId.Key);
    }

    [Fact]
    public void Type_ReturnsSeries()
    {
        Assert.Equal(ExternalIdMediaType.Series, _externalId.Type);
    }

    [Fact]
    public void UrlFormatString_ReturnsNull()
    {
        Assert.Null(_externalId.UrlFormatString);
    }
}