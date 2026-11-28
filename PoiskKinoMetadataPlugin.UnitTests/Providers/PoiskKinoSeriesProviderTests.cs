using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
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

    private const string SeriesSearchResponseWithDistinctReleaseYearsJson = """
    {
        "docs": [
            {
                "id": 101,
                "name": "Test Series",
                "isSeries": true,
                "year": 2001,
                "releaseYears": [{ "start": 2001, "end": 2004 }]
            },
            {
                "id": 202,
                "name": "Test Series",
                "isSeries": true,
                "year": 2011,
                "releaseYears": [{ "start": 2011, "end": 2019 }]
            }
        ]
    }
    """;

    [Fact]
    public async Task GetSearchResults_YearWithinReleaseYears_ReturnsOnlyMatchingSeries()
    {
        SetupSearchResponse(SeriesSearchResponseWithDistinctReleaseYearsJson);
        var provider = CreateProvider();
        var info = new SeriesInfo { Name = "Test Series", Year = 2015 };

        var results = await provider.GetSearchResults(info, CancellationToken.None);

        var match = Assert.Single(results);
        Assert.Equal("202", match.GetProviderId(ProviderNames.PoiskKino));
    }

    [Fact]
    public async Task GetSearchResults_YearOutsideReleaseYears_FallsBackToUnfiltered()
    {
        SetupSearchResponse(SeriesSearchResponseWithDistinctReleaseYearsJson);
        var provider = CreateProvider();
        var info = new SeriesInfo { Name = "Test Series", Year = 2020 };

        var results = await provider.GetSearchResults(info, CancellationToken.None);

        Assert.Equal(
            ["101", "202"],
            results.Select(result => result.GetProviderId(ProviderNames.PoiskKino)).Order());
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
    [InlineData("режиссер", null, PersonKind.Director)]
    [InlineData("Director", null, PersonKind.Director)]
    [InlineData("актер", null, PersonKind.Actor)]
    [InlineData("Actor", null, PersonKind.Actor)]
    [InlineData("продюсер", null, PersonKind.Producer)]
    [InlineData("сценарист", null, PersonKind.Writer)]
    [InlineData("композитор", null, PersonKind.Composer)]
    [InlineData(null, null, PersonKind.Actor)]
    [InlineData(null, "Director", PersonKind.Director)]
    public async Task GetMetadata_MapsPersonRole(string? profession, string? enProfession, PersonKind expected)
    {
        var json = JsonSerializer.Serialize(
            new { id = 123, persons = new[] { new { name = "Test Person", profession, enProfession } } },
            new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
        SetupMovieByIdResponse(json);
        var provider = CreateProvider();
        var info = new SeriesInfo();
        info.SetProviderId(ProviderNames.PoiskKino, "123");

        var result = await provider.GetMetadata(info, CancellationToken.None);

        var person = Assert.Single(result.People);
        Assert.Equal(expected, person.Type);
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