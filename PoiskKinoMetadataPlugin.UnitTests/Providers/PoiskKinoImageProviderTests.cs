using System.Net;
using System.Net.Http.Headers;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using PoiskKinoMetadataPlugin.UnitTests.TestData;

namespace PoiskKinoMetadataPlugin.UnitTests.Providers;

[Collection("PluginInstanceCollection")]
public class PoiskKinoImageProviderTests : PluginTestBase
{
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly Mock<IHttpClientFactory> _factoryMock;

    public PoiskKinoImageProviderTests(PluginTestFixture fixture) : base(fixture)
    {
        PluginTestFixture.SetUpPlugin(TestApiKey);

        _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("https://api.poiskkino.dev"),
            Timeout = TimeSpan.FromSeconds(120),
        };
        _factoryMock = new Mock<IHttpClientFactory>();
        _factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
    }

    private const string TestApiKey = "test-api-key";

    private PoiskKinoImageProvider CreateProvider()
    {
        var loggerFactoryMock = Fixture.LoggerFactoryMock;
        var loggerMock = new Mock<ILogger<PoiskKinoImageProvider>>();
        return new PoiskKinoImageProvider(
            _factoryMock.Object,
            loggerFactoryMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public void Supports_Movie_ReturnsTrue()
    {
        var provider = CreateProvider();
        Assert.True(provider.Supports(new Movie()));
    }

    [Fact]
    public void Supports_Series_ReturnsTrue()
    {
        var provider = CreateProvider();
        Assert.True(provider.Supports(new Series()));
    }

    [Fact]
    public void Supports_Season_ReturnsTrue()
    {
        var provider = CreateProvider();
        Assert.True(provider.Supports(new Season()));
    }

    [Fact]
    public void Supports_Episode_ReturnsFalse()
    {
        var provider = CreateProvider();
        Assert.False(provider.Supports(new Episode()));
    }

    [Fact]
    public void GetSupportedImages_ReturnsPrimaryBackdropAndLogo()
    {
        var provider = CreateProvider();

        var types = provider.GetSupportedImages(new Movie()).ToList();

        Assert.Equal(3, types.Count);
        Assert.Contains(ImageType.Primary, types);
        Assert.Contains(ImageType.Backdrop, types);
        Assert.Contains(ImageType.Logo, types);
    }

    [Fact]
    public async Task GetImages_EmptyApiKey_ReturnsEmpty()
    {
        PluginTestFixture.SetUpPlugin(null);
        var provider = CreateProvider();
        var item = new Movie();
        item.SetProviderId(ProviderNames.PoiskKino, "535341");

        var images = await provider.GetImages(item, CancellationToken.None);

        Assert.Empty(images);
    }

    [Fact]
    public async Task GetImages_ByProviderId_ReturnsPosterBackdropAndLogo()
    {
        PluginTestFixture.SetUpPlugin(TestApiKey);
        SetupMovieByIdResponse(TestJsonData.FullMovieDtoJson);
        var provider = CreateProvider();
        var item = new Movie();
        item.SetProviderId(ProviderNames.PoiskKino, "535341");

        var images = (await provider.GetImages(item, CancellationToken.None)).ToList();

        Assert.Equal(3, images.Count);
        Assert.Contains(images, i => i.Type == ImageType.Primary);
        Assert.Contains(images, i => i.Type == ImageType.Backdrop);
        Assert.Contains(images, i => i.Type == ImageType.Logo && i.Url!.Contains("oppenheimer-logo.png"));
    }

    [Fact]
    public async Task GetImages_MovieFallsBackToSearch_ReturnsImagesFromSearch()
    {
        PluginTestFixture.SetUpPlugin(TestApiKey);
        SetupMovieByIdResponse(HttpStatusCode.NotFound);
        SetupSearchResponse(TestJsonData.MixedSearchResponseJson);
        var provider = CreateProvider();
        var item = new Movie { Name = "Оппенгеймер", ProductionYear = 2023 };

        var images = (await provider.GetImages(item, CancellationToken.None)).ToList();

        Assert.Equal(3, images.Count);
        // Порядок типов детерминированный: Primary → Backdrop → Logo
        Assert.Equal(ImageType.Primary, images[0].Type);
        Assert.Equal(ImageType.Backdrop, images[1].Type);
        Assert.Equal(ImageType.Logo, images[2].Type);
        Assert.Contains("movie-poster.jpg", images[0].Url!);
        Assert.Contains("movie-backdrop.jpg", images[1].Url!);
        Assert.Contains("movie-logo.png", images[2].Url!);
    }

    [Theory]
    [InlineData("https://image.tmdb.org/t/p/w500/poster.jpg", true)]
    [InlineData("https://example.com/poster.jpg", false)]
    public async Task GetImages_TmdbUrlFiltering_RespectsConfiguration(string imageUrl, bool shouldBeFiltered)
    {
        var tmdbMovieJson = """
        {
            "id": 100,
            "name": "Test Movie",
            "poster": { "url": "__URL__" },
            "backdrop": { "url": "https://example.com/backdrop.jpg" }
        }
        """.Replace("__URL__", imageUrl);

        PluginTestFixture.SetUpPlugin(TestApiKey);
        SetupMovieByIdResponse(tmdbMovieJson);
        var provider = CreateProvider();
        var item = new Movie();
        item.SetProviderId(ProviderNames.PoiskKino, "100");

        var images = (await provider.GetImages(item, CancellationToken.None)).ToList();

        var primaryCount = images.Count(i => i.Type == ImageType.Primary);
        Assert.Equal(shouldBeFiltered ? 0 : 1, primaryCount);
    }

    // AC-6, AC-7: постер и фон вне TMDB, логотип на tmdb.org
    private const string TmdbLogoMovieJson = """
    {
        "id": 300,
        "name": "TMDB Logo Movie",
        "poster": { "url": "https://example.com/poster.jpg" },
        "backdrop": { "url": "https://example.com/backdrop.jpg" },
        "logo": { "url": "https://image.tmdb.org/t/p/original/logo.png" }
    }
    """;

    [Fact]
    public async Task GetImages_TmdbLogo_FilteredWhenIgnoreTmdbEnabled()
    {
        // AC-6, AC-11: логотип с TMDB-URL отфильтрован при IgnoreTmdbImages = true
        PluginTestFixture.SetUpPlugin(TestApiKey);
        Plugin.Instance!.Configuration.IgnoreTmdbImages = true;
        SetupMovieByIdResponse(TmdbLogoMovieJson);
        var provider = CreateProvider();
        var item = new Movie();
        item.SetProviderId(ProviderNames.PoiskKino, "300");

        var images = (await provider.GetImages(item, CancellationToken.None)).ToList();

        Assert.DoesNotContain(images, i => i.Type == ImageType.Logo);
        Assert.Contains(images, i => i.Type == ImageType.Primary);
        Assert.Contains(images, i => i.Type == ImageType.Backdrop);
    }

    [Fact]
    public async Task GetImages_TmdbLogo_ReturnedWhenIgnoreTmdbDisabled()
    {
        // AC-7: логотип возвращается как есть при IgnoreTmdbImages = false
        PluginTestFixture.SetUpPlugin(TestApiKey);
        Plugin.Instance!.Configuration.IgnoreTmdbImages = false;
        SetupMovieByIdResponse(TmdbLogoMovieJson);
        var provider = CreateProvider();
        var item = new Movie();
        item.SetProviderId(ProviderNames.PoiskKino, "300");

        var images = (await provider.GetImages(item, CancellationToken.None)).ToList();

        Assert.Equal(3, images.Count);
        Assert.Contains(images, i => i.Type == ImageType.Logo && i.Url == "https://image.tmdb.org/t/p/original/logo.png");
        Assert.Contains(images, i => i.Type == ImageType.Primary);
        Assert.Contains(images, i => i.Type == ImageType.Backdrop);

        // Восстанавливаем дефолт, чтобы флаг не протёк в другие тесты общей коллекции
        Plugin.Instance!.Configuration.IgnoreTmdbImages = true;
    }

    [Fact]
    public async Task GetImages_LogoMissing_ReturnsPosterAndBackdropWithoutLogo()
    {
        var movieJson = """
        {
            "id": 200,
            "name": "Movie Without Logo",
            "poster": { "url": "https://example.com/poster.jpg" },
            "backdrop": { "url": "https://example.com/backdrop.jpg" },
            "logo": null
        }
        """;

        PluginTestFixture.SetUpPlugin(TestApiKey);
        SetupMovieByIdResponse(movieJson);
        var provider = CreateProvider();
        var item = new Movie();
        item.SetProviderId(ProviderNames.PoiskKino, "200");

        var images = (await provider.GetImages(item, CancellationToken.None)).ToList();

        Assert.Equal(2, images.Count);
        Assert.Contains(images, i => i.Type == ImageType.Primary);
        Assert.Contains(images, i => i.Type == ImageType.Backdrop);
        Assert.DoesNotContain(images, i => i.Type == ImageType.Logo);
    }

    [Fact]
    public async Task GetImages_NoItemTitle_ReturnsEmpty()
    {
        PluginTestFixture.SetUpPlugin(TestApiKey);
        var provider = CreateProvider();
        var item = new Movie { Name = "" };

        var images = await provider.GetImages(item, CancellationToken.None);

        Assert.Empty(images);
    }

    private void SetupMovieByIdResponse(string json)
    {
        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("/movie/")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, new MediaTypeHeaderValue("application/json")),
            });
    }

    private void SetupMovieByIdResponse(HttpStatusCode statusCode)
    {
        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("/movie/")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(string.Empty),
            });
    }

    private void SetupSearchResponse(string json)
    {
        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("search")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, new MediaTypeHeaderValue("application/json")),
            });
    }

}