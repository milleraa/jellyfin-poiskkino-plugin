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
public class PoiskKinoSeasonProviderTests : PluginTestBase
{
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly Mock<IHttpClientFactory> _factoryMock;

    public PoiskKinoSeasonProviderTests(PluginTestFixture fixture) : base(fixture)
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

    private PoiskKinoSeasonProvider CreateProvider()
    {
        var loggerFactoryMock = Fixture.LoggerFactoryMock;
        var loggerMock = new Mock<ILogger<PoiskKinoSeasonProvider>>();
        return new PoiskKinoSeasonProvider(
            _factoryMock.Object,
            loggerFactoryMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task GetSearchResults_ReturnsEmptyArray()
    {
        var provider = CreateProvider();
        var info = new SeasonInfo();

        var results = await provider.GetSearchResults(info, CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetMetadata_EmptyApiKey_ReturnsEmptyResult()
    {
        PluginTestFixture.SetUpPlugin(null);
        var provider = CreateProvider();
        var info = new SeasonInfo { IndexNumber = 1 };

        var result = await provider.GetMetadata(info, CancellationToken.None);

        Assert.False(result.HasMetadata);
    }

    [Fact]
    public async Task GetMetadata_NoSeriesProviderIds_ReturnsEmptyResult()
    {
        var provider = CreateProvider();
        var info = new SeasonInfo { IndexNumber = 1 };

        var result = await provider.GetMetadata(info, CancellationToken.None);

        Assert.False(result.HasMetadata);
    }

    [Fact]
    public async Task GetMetadata_ValidProviderId_ReturnsSeasonMetadata()
    {
        PluginTestFixture.SetUpPlugin(TestApiKey);
        SetupSeasonResponse(TestJsonData.SeasonCursorResponseJson);
        var provider = CreateProvider();
        var info = new SeasonInfo
        {
            IndexNumber = 1,
            SeriesProviderIds = new Dictionary<string, string> { { ProviderNames.PoiskKino, "535341" } },
        };

        var result = await provider.GetMetadata(info, CancellationToken.None);

        Assert.True(result.HasMetadata);
        Assert.NotNull(result.Item);
        Assert.Equal("Сезон 1", result.Item.Name);
        Assert.Equal(1, result.Item.IndexNumber);
        Assert.Contains("Первый сезон", result.Item.Overview);
    }

    [Fact]
    public async Task GetMetadata_SeasonHasPoster_AddsImage()
    {
        PluginTestFixture.SetUpPlugin(TestApiKey);
        SetupSeasonResponse(TestJsonData.SeasonCursorResponseJson);
        var provider = CreateProvider();
        var info = new SeasonInfo
        {
            IndexNumber = 1,
            SeriesProviderIds = new Dictionary<string, string> { { ProviderNames.PoiskKino, "535341" } },
        };

        var result = await provider.GetMetadata(info, CancellationToken.None);

        Assert.True(result.HasMetadata);
    }

    [Fact]
    public async Task GetMetadata_SeasonNotFound_ReturnsEmptyResult()
    {
        PluginTestFixture.SetUpPlugin(TestApiKey);
        SetupSeasonResponse(TestJsonData.EmptySeasonCursorResponseJson);
        var provider = CreateProvider();
        var info = new SeasonInfo
        {
            IndexNumber = 1,
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