using System.Net;
using System.Net.Http.Headers;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using PoiskKinoMetadataPlugin.UnitTests.TestData;

namespace PoiskKinoMetadataPlugin.UnitTests.Providers;

[Collection("PluginInstanceCollection")]
public class PoiskKinoSeriesProviderTests : PluginTestBase
{
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly Mock<IHttpClientFactory> _factoryMock;

    public PoiskKinoSeriesProviderTests(PluginTestFixture fixture) : base(fixture)
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

    private PoiskKinoSeriesProvider CreateProvider()
    {
        var loggerFactoryMock = Fixture.LoggerFactoryMock;
        var loggerMock = new Mock<ILogger<PoiskKinoSeriesProvider>>();
        return new PoiskKinoSeriesProvider(
            _factoryMock.Object,
            loggerFactoryMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task GetSearchResults_EmptyTitle_ReturnsEmptyList()
    {
        var provider = CreateProvider();
        var info = new SeriesInfo { Name = "" };

        var results = await provider.GetSearchResults(info, CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetSearchResults_FiltersOnlySeries()
    {
        PluginTestFixture.SetUpPlugin(TestApiKey);
        SetupSearchResponse(TestJsonData.MixedSearchResponseJson);
        var provider = CreateProvider();
        var info = new SeriesInfo { Name = "Test" };

        var results = (await provider.GetSearchResults(info, CancellationToken.None)).ToList();

        Assert.Single(results);
        Assert.Equal(404900, int.Parse(results[0].GetProviderId(ProviderNames.PoiskKino)!));
    }

    [Fact]
    public async Task GetSearchResults_MapsFieldsCorrectly()
    {
        PluginTestFixture.SetUpPlugin(TestApiKey);
        SetupSearchResponse(TestJsonData.SeriesSearchResponseJson);
        var provider = CreateProvider();
        var info = new SeriesInfo { Name = "Игра престолов" };

        var results = (await provider.GetSearchResults(info, CancellationToken.None)).ToList();

        Assert.Single(results);
        Assert.Equal("Игра престолов", results[0].Name);
        Assert.Equal(2011, results[0].ProductionYear);
        Assert.Equal("404900", results[0].GetProviderId(ProviderNames.PoiskKino));
    }

    [Fact]
    public async Task GetMetadata_EmptyApiKey_ReturnsEmptyResult()
    {
        PluginTestFixture.SetUpPlugin(null);
        var provider = CreateProvider();
        var info = new SeriesInfo { Name = "Test" };

        var result = await provider.GetMetadata(info, CancellationToken.None);

        Assert.False(result.HasMetadata);
    }

    [Fact]
    public async Task GetMetadata_ByProviderId_ReturnsSeriesMetadata()
    {
        PluginTestFixture.SetUpPlugin(TestApiKey);
        SetupMovieByIdResponse(TestJsonData.FullMovieDtoJson);
        var provider = CreateProvider();
        var info = new SeriesInfo();
        info.SetProviderId(ProviderNames.PoiskKino, "535341");

        var result = await provider.GetMetadata(info, CancellationToken.None);

        Assert.True(result.HasMetadata);
        Assert.NotNull(result.Item);
        Assert.IsType<Series>(result.Item);
        Assert.Equal("Оппенгеймер", result.Item.Name);
    }

    [Fact]
    public async Task GetMetadata_MapsGenres()
    {
        PluginTestFixture.SetUpPlugin(TestApiKey);
        SetupMovieByIdResponse(TestJsonData.FullMovieDtoJson);
        var provider = CreateProvider();
        var info = new SeriesInfo();
        info.SetProviderId(ProviderNames.PoiskKino, "535341");

        var result = await provider.GetMetadata(info, CancellationToken.None);

        Assert.NotNull(result.Item!.Genres);
        Assert.Contains("драма", result.Item.Genres);
    }

    [Fact]
    public async Task GetMetadata_MapsRatings()
    {
        PluginTestFixture.SetUpPlugin(TestApiKey);
        SetupMovieByIdResponse(TestJsonData.FullMovieDtoJson);
        var provider = CreateProvider();
        var info = new SeriesInfo();
        info.SetProviderId(ProviderNames.PoiskKino, "535341");

        var result = await provider.GetMetadata(info, CancellationToken.None);

        Assert.Equal(8.4f, result.Item!.CommunityRating);
        Assert.Equal(8.6f, result.Item!.CriticRating);
    }

    [Theory]
    [InlineData("режиссер", PersonKind.Director)]
    [InlineData("Director", PersonKind.Director)]
    [InlineData("актер", PersonKind.Actor)]
    [InlineData("Actor", PersonKind.Actor)]
    [InlineData("продюсер", PersonKind.Producer)]
    [InlineData("сценарист", PersonKind.Writer)]
    [InlineData("композитор", PersonKind.Composer)]
    [InlineData(null, PersonKind.Actor)]
    public void GetPersonKind_VariousProfessions_MapsCorrectly(string? profession, PersonKind expected)
    {
        var method = typeof(PoiskKinoSeriesProvider).GetMethod(
            "GetPersonKind", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var result = method!.Invoke(null, [profession]);

        Assert.Equal(expected, result);
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

}