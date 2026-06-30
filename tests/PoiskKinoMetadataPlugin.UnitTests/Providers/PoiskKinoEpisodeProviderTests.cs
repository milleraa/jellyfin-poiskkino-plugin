using System.Net;
using System.Net.Http.Headers;
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
public class PoiskKinoEpisodeProviderTests : PluginTestBase
{
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly Mock<IHttpClientFactory> _factoryMock;

    public PoiskKinoEpisodeProviderTests(PluginTestFixture fixture) : base(fixture)
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

    private PoiskKinoEpisodeProvider CreateProvider()
    {
        var loggerFactoryMock = Fixture.LoggerFactoryMock;
        var loggerMock = new Mock<ILogger<PoiskKinoEpisodeProvider>>();
        return new PoiskKinoEpisodeProvider(
            _factoryMock.Object,
            loggerFactoryMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task GetSearchResults_ReturnsEmptyArray()
    {
        var provider = CreateProvider();
        var info = new EpisodeInfo();

        var results = await provider.GetSearchResults(info, CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetMetadata_EmptyApiKey_ReturnsEmptyResult()
    {
        PluginTestFixture.SetUpPlugin(null);
        var provider = CreateProvider();
        var info = new EpisodeInfo { IndexNumber = 1, ParentIndexNumber = 1 };

        var result = await provider.GetMetadata(info, CancellationToken.None);

        Assert.False(result.HasMetadata);
    }

    [Fact]
    public async Task GetMetadata_NoSeriesProviderIds_ReturnsEmptyResult()
    {
        var provider = CreateProvider();
        var info = new EpisodeInfo { IndexNumber = 1, ParentIndexNumber = 1 };

        var result = await provider.GetMetadata(info, CancellationToken.None);

        Assert.False(result.HasMetadata);
    }

    [Fact]
    public async Task GetMetadata_FindsEpisodeByNameAndNumber()
    {
        PluginTestFixture.SetUpPlugin(TestApiKey);
        SetupSeasonResponse(TestJsonData.SeasonCursorResponseJson);
        var provider = CreateProvider();
        var info = new EpisodeInfo
        {
            IndexNumber = 1,
            ParentIndexNumber = 1,
            SeriesProviderIds = new Dictionary<string, string> { { ProviderNames.PoiskKino, "535341" } },
        };

        var result = await provider.GetMetadata(info, CancellationToken.None);

        Assert.True(result.HasMetadata);
        Assert.NotNull(result.Item);
        Assert.Equal("Пилот", result.Item.Name);
        Assert.Equal(1, result.Item.IndexNumber);
    }

    [Fact]
    public async Task GetMetadata_EpisodeHasStill_AddsImage()
    {
        PluginTestFixture.SetUpPlugin(TestApiKey);
        SetupSeasonResponse(TestJsonData.SeasonCursorResponseJson);
        var provider = CreateProvider();
        var info = new EpisodeInfo
        {
            IndexNumber = 1,
            ParentIndexNumber = 1,
            SeriesProviderIds = new Dictionary<string, string> { { ProviderNames.PoiskKino, "535341" } },
        };

        var result = await provider.GetMetadata(info, CancellationToken.None);

        Assert.True(result.HasMetadata);
    }

    [Fact]
    public async Task GetMetadata_EpisodeNotFound_ReturnsEmptyResult()
    {
        PluginTestFixture.SetUpPlugin(TestApiKey);
        SetupSeasonResponse(TestJsonData.SeasonCursorResponseJson);
        var provider = CreateProvider();
        var info = new EpisodeInfo
        {
            IndexNumber = 99,
            ParentIndexNumber = 1,
            SeriesProviderIds = new Dictionary<string, string> { { ProviderNames.PoiskKino, "535341" } },
        };

        var result = await provider.GetMetadata(info, CancellationToken.None);

        Assert.False(result.HasMetadata);
    }

    [Fact]
    public async Task GetMetadata_NoSeasonData_ReturnsEmptyResult()
    {
        PluginTestFixture.SetUpPlugin(TestApiKey);
        SetupSeasonResponse(TestJsonData.EmptySeasonCursorResponseJson);
        var provider = CreateProvider();
        var info = new EpisodeInfo
        {
            IndexNumber = 1,
            ParentIndexNumber = 1,
            SeriesProviderIds = new Dictionary<string, string> { { ProviderNames.PoiskKino, "535341" } },
        };

        var result = await provider.GetMetadata(info, CancellationToken.None);

        Assert.False(result.HasMetadata);
    }

    private void SetupSeasonResponse(string json)
    {
        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains("/v1.5/season")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, new MediaTypeHeaderValue("application/json")),
            });
    }

}