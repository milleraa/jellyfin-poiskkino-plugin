namespace PoiskKinoMetadataPlugin.UnitTests.Helpers;

public class ImageUrlHelperTests
{
    [Theory]
    [InlineData("https://image.tmdb.org/t/p/w500/abc.jpg", true)]
    [InlineData("https://image.tmdb.org/original/abc.jpg", true)]
    [InlineData("http://image.tmdb.org/t/p/w500/abc.jpg", true)]
    [InlineData("HTTPS://IMAGE.TMDB.ORG/ABC.jpg", true)]
    [InlineData("https://example.com/image.jpg", false)]
    [InlineData("https://image.kinopoisk.com/poster.jpg", false)]
    [InlineData("https://cdn.myservice.com/tmdb.org/fake.jpg", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsTmdbUrl_VariousUrls_ReturnsCorrect(string? url, bool expected)
    {
        var result = ImageUrlHelper.IsTmdbUrl(url);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ShouldIgnoreTmdbImages_WhenPluginNotInitialized_ReturnsFalse()
    {
        var result = ImageUrlHelper.ShouldIgnoreTmdbImages();

        Assert.False(result);
    }

    [Fact]
    public void ShouldFilterUrl_WhenUrlIsNull_ReturnsFalse()
    {
        var result = ImageUrlHelper.ShouldFilterUrl(null);

        Assert.False(result);
    }

    [Fact]
    public void ShouldFilterUrl_WhenUrlIsEmpty_ReturnsFalse()
    {
        var result = ImageUrlHelper.ShouldFilterUrl("");

        Assert.False(result);
    }

    [Fact]
    public void ShouldFilterUrl_WhenNotTmdbUrl_ReturnsFalse()
    {
        var result = ImageUrlHelper.ShouldFilterUrl("https://example.com/image.jpg");

        Assert.False(result);
    }
}