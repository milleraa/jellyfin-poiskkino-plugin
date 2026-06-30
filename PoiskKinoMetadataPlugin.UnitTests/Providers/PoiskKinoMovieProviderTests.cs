using System.Net;
using System.Net.Http.Headers;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using PoiskKinoMetadataPlugin.UnitTests.TestData;

namespace PoiskKinoMetadataPlugin.UnitTests.Providers;

[Collection("PluginInstanceCollection")]
public class PoiskKinoMovieProviderTests : PluginTestBase
{
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly Mock<IHttpClientFactory> _factoryMock;

    public PoiskKinoMovieProviderTests(PluginTestFixture fixture) : base(fixture)
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

    private PoiskKinoMovieProvider CreateProvider()
    {
        var loggerFactoryMock = Fixture.LoggerFactoryMock;
        var loggerMock = new Mock<ILogger<PoiskKinoMovieProvider>>();
        return new PoiskKinoMovieProvider(
            _factoryMock.Object,
            loggerFactoryMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task GetSearchResults_EmptyTitle_ReturnsEmptyList()
    {
        var provider = CreateProvider();
        var info = new MovieInfo { Name = "" };

        var results = await provider.GetSearchResults(info, CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetSearchResults_NoApiKey_ReturnsEmptyList()
    {
        PluginTestFixture.SetUpPlugin(null);
        SetupSearchResponse(HttpStatusCode.OK, TestJsonData.MixedSearchResponseJson);
        var provider = CreateProvider();
        var info = new MovieInfo { Name = "Test" };

        var results = await provider.GetSearchResults(info, CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetSearchResults_FiltersOnlyMovies()
    {
        PluginTestFixture.SetUpPlugin(TestApiKey);
        SetupSearchResponse(HttpStatusCode.OK, TestJsonData.MixedSearchResponseJson);
        var provider = CreateProvider();
        var info = new MovieInfo { Name = "Test" };

        var results = (await provider.GetSearchResults(info, CancellationToken.None)).ToList();

        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.NotEqual("404900", r.GetProviderId("PoiskKino")));
    }

    [Fact]
    public async Task GetSearchResults_MapsFieldsCorrectly()
    {
        PluginTestFixture.SetUpPlugin(TestApiKey);
        SetupSearchResponse(HttpStatusCode.OK, TestJsonData.SearchResponseJson);
        var provider = CreateProvider();
        var info = new MovieInfo { Name = "Оппенгеймер" };

        var results = (await provider.GetSearchResults(info, CancellationToken.None)).ToList();

        Assert.Single(results);
        var result = results[0];
        Assert.Equal("Оппенгеймер", result.Name);
        Assert.Equal(2023, result.ProductionYear);
        Assert.Contains("Роберта Оппенгеймера", result.Overview);
        Assert.Equal("535341", result.GetProviderId(ProviderNames.PoiskKino));
        Assert.Equal("tt15398776", result.GetProviderId("Imdb"));
        Assert.Equal("48e8d0acb0f62d8585101798eaeceec5", result.GetProviderId("Kinopoisk"));
        Assert.Equal("872585", result.GetProviderId("Tmdb"));
        Assert.NotNull(result.ImageUrl);
    }

    [Fact]
    public async Task GetSearchResults_ResponseIsNull_ReturnsEmpty()
    {
        PluginTestFixture.SetUpPlugin(TestApiKey);
        SetupSearchResponse(HttpStatusCode.NotFound);
        var provider = CreateProvider();
        var info = new MovieInfo { Name = "NotFound" };

        var results = await provider.GetSearchResults(info, CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetMetadata_EmptyApiKey_ReturnsEmptyResult()
    {
        PluginTestFixture.SetUpPlugin(null);
        var provider = CreateProvider();
        var info = new MovieInfo { Name = "Test" };

        var result = await provider.GetMetadata(info, CancellationToken.None);

        Assert.False(result.HasMetadata);
    }

    [Fact]
    public async Task GetMetadata_ByProviderId_ReturnsMetadata()
    {
        PluginTestFixture.SetUpPlugin(TestApiKey);
        SetupMovieByIdResponse(HttpStatusCode.OK, TestJsonData.FullMovieDtoJson);
        var provider = CreateProvider();
        var info = new MovieInfo();
        info.SetProviderId(ProviderNames.PoiskKino, "535341");

        var result = await provider.GetMetadata(info, CancellationToken.None);

        Assert.True(result.HasMetadata);
        Assert.NotNull(result.Item);
        Assert.Equal("Оппенгеймер", result.Item.Name);
        Assert.Contains("Роберта Оппенгеймера", result.Item.Overview);
        Assert.Equal(2023, result.Item.ProductionYear);
    }

    [Fact]
    public async Task GetMetadata_MapsGenres()
    {
        PluginTestFixture.SetUpPlugin(TestApiKey);
        SetupMovieByIdResponse(HttpStatusCode.OK, TestJsonData.FullMovieDtoJson);
        var provider = CreateProvider();
        var info = new MovieInfo();
        info.SetProviderId(ProviderNames.PoiskKino, "535341");

        var result = await provider.GetMetadata(info, CancellationToken.None);

        Assert.NotNull(result.Item!.Genres);
        Assert.Equal(3, result.Item.Genres.Length);
        Assert.Contains("драма", result.Item.Genres);
        Assert.Contains("биография", result.Item.Genres);
        Assert.Contains("история", result.Item.Genres);
    }

    [Fact]
    public async Task GetMetadata_MapsRatings()
    {
        PluginTestFixture.SetUpPlugin(TestApiKey);
        SetupMovieByIdResponse(HttpStatusCode.OK, TestJsonData.FullMovieDtoJson);
        var provider = CreateProvider();
        var info = new MovieInfo();
        info.SetProviderId(ProviderNames.PoiskKino, "535341");

        var result = await provider.GetMetadata(info, CancellationToken.None);

        Assert.NotNull(result.Item);
        Assert.Equal(8.4f, result.Item.CommunityRating);
        Assert.Equal(8.6f, result.Item.CriticRating);
    }

    [Fact]
    public async Task GetMetadata_MapsPersons()
    {
        PluginTestFixture.SetUpPlugin(TestApiKey);
        SetupMovieByIdResponse(HttpStatusCode.OK, TestJsonData.FullMovieDtoJson);
        var provider = CreateProvider();
        var info = new MovieInfo();
        info.SetProviderId(ProviderNames.PoiskKino, "535341");

        var result = await provider.GetMetadata(info, CancellationToken.None);

        Assert.True(result.People.Count >= 2);
    }

    [Fact]
    public async Task GetMetadata_MapsProviderIds()
    {
        PluginTestFixture.SetUpPlugin(TestApiKey);
        SetupMovieByIdResponse(HttpStatusCode.OK, TestJsonData.FullMovieDtoJson);
        var provider = CreateProvider();
        var info = new MovieInfo();
        info.SetProviderId(ProviderNames.PoiskKino, "535341");

        var result = await provider.GetMetadata(info, CancellationToken.None);

        Assert.NotNull(result.Item);
        Assert.Equal("535341", result.Item.GetProviderId(ProviderNames.PoiskKino));
        Assert.Equal("tt15398776", result.Item.GetProviderId("Imdb"));
        Assert.Equal("48e8d0acb0f62d8585101798eaeceec5", result.Item.GetProviderId("Kinopoisk"));
        Assert.Equal("872585", result.Item.GetProviderId("Tmdb"));
    }

    [Fact]
    public async Task GetMetadata_NoProviderId_FallsBackToSearch()
    {
        PluginTestFixture.SetUpPlugin(TestApiKey);
        SetupSearchResponse(HttpStatusCode.OK, TestJsonData.SearchResponseJson);
        SetupMovieByIdResponse(HttpStatusCode.OK, TestJsonData.FullMovieDtoJson);
        var provider = CreateProvider();
        var info = new MovieInfo { Name = "Оппенгеймер" };

        var result = await provider.GetMetadata(info, CancellationToken.None);

        Assert.True(result.HasMetadata);
        Assert.NotNull(result.Item);
        Assert.Equal("Оппенгеймер", result.Item.Name);
    }

    [Theory]
    [InlineData("режиссер", PersonKind.Director)]
    [InlineData("Director", PersonKind.Director)]
    [InlineData("director", PersonKind.Director)]
    [InlineData("актер", PersonKind.Actor)]
    [InlineData("актриса", PersonKind.Actor)]
    [InlineData("Actor", PersonKind.Actor)]
    [InlineData("actress", PersonKind.Actor)]
    [InlineData("продюсер", PersonKind.Producer)]
    [InlineData("Producer", PersonKind.Producer)]
    [InlineData("сценарист", PersonKind.Writer)]
    [InlineData("Writer", PersonKind.Writer)]
    [InlineData("Screenplay writer", PersonKind.Writer)]
    [InlineData("композитор", PersonKind.Composer)]
    [InlineData("Composer", PersonKind.Composer)]
    [InlineData("unknown role", PersonKind.Actor)]
    [InlineData("", PersonKind.Actor)]
    [InlineData(null, PersonKind.Actor)]
    public void GetPersonKind_VariousProfessions_MapsCorrectly(string? profession, PersonKind expected)
    {
        var provider = CreateProvider();
        var info = new MovieInfo { Name = "Test" };

        // Access private method via reflection to test GetPersonKind
        var method = typeof(PoiskKinoMovieProvider).GetMethod(
            "GetPersonKind", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var result = method!.Invoke(null, [profession]);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task GetImageResponse_EmptyUrl_ReturnsBadRequest()
    {
        var provider = CreateProvider();

        var response = await provider.GetImageResponse("", CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private void SetupSearchResponse(HttpStatusCode statusCode, string? json = null)
    {
        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("search")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = json != null
                    ? new StringContent(json, new MediaTypeHeaderValue("application/json"))
                    : new StringContent(string.Empty),
            });
    }

    private void SetupMovieByIdResponse(HttpStatusCode statusCode, string? json = null)
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
                Content = json != null
                    ? new StringContent(json, new MediaTypeHeaderValue("application/json"))
                    : new StringContent(string.Empty),
            });
    }

}