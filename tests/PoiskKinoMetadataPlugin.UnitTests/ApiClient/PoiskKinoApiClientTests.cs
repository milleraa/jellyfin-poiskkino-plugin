using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using PoiskKinoMetadataPlugin.UnitTests.TestData;

namespace PoiskKinoMetadataPlugin.UnitTests.ApiClient;

public class PoiskKinoApiClientTests
{
    private const string TestApiKey = "test-api-key";

    private static Mock<HttpMessageHandler> CreateHandlerMock(HttpStatusCode statusCode, HttpContent? content = null)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = content ?? new StringContent(string.Empty),
            });
        return handlerMock;
    }

    private static Mock<IHttpClientFactory> CreateHttpClientFactoryMock(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.poiskkino.dev"),
            Timeout = TimeSpan.FromSeconds(120),
        };
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Jellyfin-PoiskKino-Plugin/1.0");

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
        return factoryMock;
    }

    private PoiskKinoApiClient CreateClient(Mock<HttpMessageHandler> handlerMock)
    {
        var factoryMock = CreateHttpClientFactoryMock(handlerMock.Object);
        var loggerMock = new Mock<ILogger<PoiskKinoApiClient>>();
        return new PoiskKinoApiClient(factoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task SearchAsync_EmptyApiKey_ReturnsNull()
    {
        var handlerMock = CreateHandlerMock(HttpStatusCode.OK);
        var client = CreateClient(handlerMock);

        var result = await client.SearchAsync("Test", null, "", CancellationToken.None);

        Assert.Null(result);
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Never(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SearchAsync_Success_ReturnsDeserializedData()
    {
        var handlerMock = CreateHandlerMock(
            HttpStatusCode.OK,
            new StringContent(TestJsonData.SearchResponseJson, new MediaTypeHeaderValue("application/json")));
        var client = CreateClient(handlerMock);

        var result = await client.SearchAsync("Test", null, TestApiKey, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result.Total);
        Assert.NotNull(result.Docs);
        Assert.Equal(2, result.Docs.Count);
    }

    [Fact]
    public async Task SearchAsync_WithYear_IncludesYearInUrl()
    {
        HttpRequestMessage? capturedRequest = null;
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(TestJsonData.SearchResponseJson, new MediaTypeHeaderValue("application/json")),
            });

        var client = CreateClient(handlerMock);

        await client.SearchAsync("Test", 2023, TestApiKey, CancellationToken.None);

        Assert.NotNull(capturedRequest);
        Assert.Contains("year=2023", capturedRequest!.RequestUri!.ToString());
    }

    [Fact]
    public async Task SearchAsync_CachesResult()
    {
        var handlerMock = CreateHandlerMock(
            HttpStatusCode.OK,
            new StringContent(TestJsonData.SearchResponseJson, new MediaTypeHeaderValue("application/json")));
        var client = CreateClient(handlerMock);

        var result1 = await client.SearchAsync("CacheTest", null, TestApiKey, CancellationToken.None);
        var result2 = await client.SearchAsync("CacheTest", null, TestApiKey, CancellationToken.None);

        Assert.NotNull(result1);
        Assert.NotNull(result2);
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SearchAsync_Unauthorized_ReturnsNull()
    {
        var handlerMock = CreateHandlerMock(
            HttpStatusCode.Unauthorized,
            new StringContent("{\"message\":\"Invalid API key\"}", new MediaTypeHeaderValue("application/json")));
        var client = CreateClient(handlerMock);

        var result = await client.SearchAsync("Test", null, TestApiKey, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task SearchAsync_Forbidden_ReturnsNull()
    {
        var handlerMock = CreateHandlerMock(
            HttpStatusCode.Forbidden,
            new StringContent("{\"message\":\"Daily limit exceeded\"}", new MediaTypeHeaderValue("application/json")));
        var client = CreateClient(handlerMock);

        var result = await client.SearchAsync("Test", null, TestApiKey, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task SearchAsync_TooManyRequests_ReturnsNull()
    {
        var handlerMock = CreateHandlerMock(
            HttpStatusCode.TooManyRequests,
            new StringContent("{\"message\":\"Rate limit exceeded\"}", new MediaTypeHeaderValue("application/json")));
        var client = CreateClient(handlerMock);

        var result = await client.SearchAsync("Test", null, TestApiKey, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task SearchAsync_NotFound_CachesNegativeResult()
    {
        var handlerMock = CreateHandlerMock(HttpStatusCode.NotFound);
        var client = CreateClient(handlerMock);

        var result1 = await client.SearchAsync("NotFound", null, TestApiKey, CancellationToken.None);
        var result2 = await client.SearchAsync("NotFound", null, TestApiKey, CancellationToken.None);

        Assert.Null(result1);
        Assert.Null(result2);
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SearchAsync_OtherErrorStatus_ReturnsNull()
    {
        var handlerMock = CreateHandlerMock(HttpStatusCode.InternalServerError);
        var client = CreateClient(handlerMock);

        var result = await client.SearchAsync("Test", null, TestApiKey, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task SearchAsync_Timeout_ReturnsNull()
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException());

        var client = CreateClient(handlerMock);

        var result = await client.SearchAsync("Test", null, TestApiKey, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetMovieByIdAsync_EmptyApiKey_ReturnsNull()
    {
        var handlerMock = CreateHandlerMock(HttpStatusCode.OK);
        var client = CreateClient(handlerMock);

        var result = await client.GetMovieByIdAsync(535341, "", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetMovieByIdAsync_Success_ReturnsDeserializedData()
    {
        var handlerMock = CreateHandlerMock(
            HttpStatusCode.OK,
            new StringContent(TestJsonData.FullMovieDtoJson, new MediaTypeHeaderValue("application/json")));
        var client = CreateClient(handlerMock);

        var result = await client.GetMovieByIdAsync(535341, TestApiKey, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(535341, result.Id);
        Assert.Equal("Оппенгеймер", result.Name);
        Assert.Equal(2023, result.Year);
        Assert.NotNull(result.Rating);
        Assert.Equal(8.6, result.Rating.Kp);
    }

    [Fact]
    public async Task GetMovieByIdAsync_CachesResult()
    {
        var handlerMock = CreateHandlerMock(
            HttpStatusCode.OK,
            new StringContent(TestJsonData.FullMovieDtoJson, new MediaTypeHeaderValue("application/json")));
        var client = CreateClient(handlerMock);

        var result1 = await client.GetMovieByIdAsync(535341, TestApiKey, CancellationToken.None);
        var result2 = await client.GetMovieByIdAsync(535341, TestApiKey, CancellationToken.None);

        Assert.NotNull(result1);
        Assert.NotNull(result2);
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetMovieByIdAsync_Unauthorized_ReturnsNull()
    {
        var handlerMock = CreateHandlerMock(
            HttpStatusCode.Unauthorized,
            new StringContent("{\"message\":\"Invalid API key\"}", new MediaTypeHeaderValue("application/json")));
        var client = CreateClient(handlerMock);

        var result = await client.GetMovieByIdAsync(535341, TestApiKey, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetMovieByIdAsync_NotFound_CachesNegativeResult()
    {
        var handlerMock = CreateHandlerMock(HttpStatusCode.NotFound);
        var client = CreateClient(handlerMock);

        var result1 = await client.GetMovieByIdAsync(9999, TestApiKey, CancellationToken.None);
        var result2 = await client.GetMovieByIdAsync(9999, TestApiKey, CancellationToken.None);

        Assert.Null(result1);
        Assert.Null(result2);
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetSeasonAsync_EmptyApiKey_ReturnsNull()
    {
        var handlerMock = CreateHandlerMock(HttpStatusCode.OK);
        var client = CreateClient(handlerMock);

        var result = await client.GetSeasonAsync(535341, 1, "", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetSeasonAsync_Success_ReturnsSeasonFromDocs()
    {
        var handlerMock = CreateHandlerMock(
            HttpStatusCode.OK,
            new StringContent(TestJsonData.SeasonCursorResponseJson, new MediaTypeHeaderValue("application/json")));
        var client = CreateClient(handlerMock);

        var result = await client.GetSeasonAsync(535341, 1, TestApiKey, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(535341, result.MovieId);
        Assert.Equal(1, result.Number);
        Assert.Equal("Сезон 1", result.Name);
        Assert.NotNull(result.Episodes);
        Assert.Equal(3, result.Episodes.Count);
    }

    [Fact]
    public async Task GetSeasonAsync_CachesResult()
    {
        var handlerMock = CreateHandlerMock(
            HttpStatusCode.OK,
            new StringContent(TestJsonData.SeasonCursorResponseJson, new MediaTypeHeaderValue("application/json")));
        var client = CreateClient(handlerMock);

        var result1 = await client.GetSeasonAsync(535341, 1, TestApiKey, CancellationToken.None);
        var result2 = await client.GetSeasonAsync(535341, 1, TestApiKey, CancellationToken.None);

        Assert.NotNull(result1);
        Assert.NotNull(result2);
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetSeasonAsync_EmptyDocs_ReturnsNull()
    {
        var handlerMock = CreateHandlerMock(
            HttpStatusCode.OK,
            new StringContent(TestJsonData.EmptySeasonCursorResponseJson, new MediaTypeHeaderValue("application/json")));
        var client = CreateClient(handlerMock);

        var result = await client.GetSeasonAsync(535341, 1, TestApiKey, CancellationToken.None);

        Assert.Null(result);
    }
}