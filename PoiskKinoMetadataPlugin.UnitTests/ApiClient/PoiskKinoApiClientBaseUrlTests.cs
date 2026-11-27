using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace PoiskKinoMetadataPlugin.UnitTests.ApiClient;

/// <summary>
/// Тесты применения настраиваемого базового URL в <see cref="PoiskKinoApiClient"/>
/// (AC-1, AC-2, AC-4, AC-5, AC-9 фичи `eu-api-endpoint`).
/// </summary>
/// <remarks>
/// Фактический адрес проверяется по захваченному <see cref="HttpRequestMessage.RequestUri"/>:
/// клиент сам ставит <c>BaseAddress</c> на экземпляр <c>HttpClient</c>, полученный из фабрики.
/// </remarks>
public class PoiskKinoApiClientBaseUrlTests
{
    private const string TestApiKey = "test-api-key";
    private const string DefaultHost = "api.poiskkino.dev";
    private const string CustomHost = "eu-api.poiskkino.dev";
    private const string CustomBaseUrl = "https://eu-api.poiskkino.dev";

    private const string EmptySearchResponseJson = "{\"docs\":[],\"total\":0}";

    private readonly List<HttpRequestMessage> _capturedRequests = [];
    private readonly Mock<ILogger<PoiskKinoApiClient>> _loggerMock = new();

    // AC-1, AC-2: двухаргументный конструктор остаётся валидным и даёт прежний хост.
    [Fact]
    public async Task Constructor_WithoutBaseUrl_UsesDefaultHost()
    {
        var client = CreateClient();

        await client.SearchAsync("Test", null, TestApiKey);

        var request = Assert.Single(_capturedRequests);
        Assert.Equal(Uri.UriSchemeHttps, request.RequestUri!.Scheme);
        Assert.Equal(DefaultHost, request.RequestUri.Host);
        Assert.Equal(443, request.RequestUri.Port);
        Assert.Equal("/v1.5/movie/search", request.RequestUri.AbsolutePath);

        VerifyLog(LogLevel.Information, Times.Once());
        VerifyLog(LogLevel.Warning, Times.Never());
        VerifyLoggedMessageContains(LogLevel.Information, DefaultHost);
    }

    // AC-5: сконфигурированный хост попадает в URL всех трёх запросов, пути не изменились.
    [Fact]
    public async Task CustomBaseUrl_AppliedToAllRequests()
    {
        var client = CreateClientWith(CustomBaseUrl);

        await client.SearchAsync("Test", null, TestApiKey);
        await client.GetMovieByIdAsync(535341, TestApiKey);
        await client.GetSeasonAsync(535341, 1, TestApiKey);

        Assert.Equal(3, _capturedRequests.Count);
        Assert.All(_capturedRequests, request =>
        {
            Assert.Equal(Uri.UriSchemeHttps, request.RequestUri!.Scheme);
            Assert.Equal(CustomHost, request.RequestUri.Host);
            Assert.Equal(443, request.RequestUri.Port);
        });

        Assert.Equal("/v1.5/movie/search", _capturedRequests[0].RequestUri!.AbsolutePath);
        Assert.Equal("/v1.5/movie/535341", _capturedRequests[1].RequestUri!.AbsolutePath);
        Assert.Equal("/v1.5/season", _capturedRequests[2].RequestUri!.AbsolutePath);

        VerifyLog(LogLevel.Information, Times.Once());
        VerifyLog(LogLevel.Warning, Times.Never());
        VerifyLoggedMessageContains(LogLevel.Information, CustomHost);
    }

    // AC-3 (интеграция) + AC-5: значение нормализуется резолвером внутри клиента.
    [Theory]
    [InlineData("eu-api.poiskkino.dev")]
    [InlineData("https://eu-api.poiskkino.dev/")]
    [InlineData("  https://eu-api.poiskkino.dev  ")]
    public async Task BaseUrlInputVariants_NormalizedBeforeRequest(string rawApiBaseUrl)
    {
        var client = CreateClientWith(rawApiBaseUrl);

        await client.SearchAsync("Test", null, TestApiKey);

        var request = Assert.Single(_capturedRequests);
        Assert.Equal(Uri.UriSchemeHttps, request.RequestUri!.Scheme);
        Assert.Equal(CustomHost, request.RequestUri.Host);
        VerifyLog(LogLevel.Warning, Times.Never());
    }

    // AC-4 + AC-9: невалидное значение — дефолт, один Warning, без исключений.
    [Theory]
    [InlineData("http://evil.example")]
    [InlineData("not a url")]
    [InlineData("https://api.poiskkino.dev/v1.5")]
    public async Task InvalidBaseUrl_FallsBackToDefaultWithSingleWarning(string rawApiBaseUrl)
    {
        var client = CreateClientWith(rawApiBaseUrl);

        var exception = await Record.ExceptionAsync(() => client.SearchAsync("Test", null, TestApiKey));

        Assert.Null(exception);
        var request = Assert.Single(_capturedRequests);
        Assert.Equal(Uri.UriSchemeHttps, request.RequestUri!.Scheme);
        Assert.Equal(DefaultHost, request.RequestUri.Host);

        VerifyLog(LogLevel.Warning, Times.Once());
        VerifyLog(LogLevel.Information, Times.Once());
    }

    // AC-4: создание клиента не бросает исключений ни на каком входе.
    [Theory]
    [InlineData("")]
    [InlineData("http://evil.example")]
    [InlineData("://")]
    [InlineData("https://user:secret@api.poiskkino.dev")]
    [InlineData("https://api.poiskkino.dev?x=1")]
    [InlineData("https://api.poiskkino.dev#frag")]
    [InlineData("api.poiskkino.dev:8443")]
    public void Constructor_AnyInput_DoesNotThrow(string rawApiBaseUrl)
    {
        var exception = Record.Exception(() => CreateClientWith(rawApiBaseUrl));

        Assert.Null(exception);
    }

    // AC-4 (порт): явный порт сохраняется в фактическом адресе запроса.
    [Fact]
    public async Task CustomPort_PreservedInRequestUri()
    {
        var client = CreateClientWith("https://eu-api.poiskkino.dev:8443");

        await client.SearchAsync("Test", null, TestApiKey);

        var request = Assert.Single(_capturedRequests);
        Assert.Equal(CustomHost, request.RequestUri!.Host);
        Assert.Equal(8443, request.RequestUri.Port);
    }

    // AC-9: сырое значение в Warning усечено до 200 символов.
    [Fact]
    public void Constructor_InvalidLongBaseUrl_TruncatesRawValueInWarning()
    {
        var rawApiBaseUrl = new string('a', 500);

        _ = CreateClientWith(rawApiBaseUrl);

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) =>
                    state.ToString()!.Contains(new string('a', 200), StringComparison.Ordinal) &&
                    !state.ToString()!.Contains(new string('a', 201), StringComparison.Ordinal)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        VerifyLog(LogLevel.Information, Times.Once());
    }

    // AC-9 (риск A6): пароль из userinfo не попадает в Warning, хост остаётся в сообщении.
    [Theory]
    [InlineData("https://admin:sup3r-secret@api.poiskkino.dev")]
    [InlineData("admin:sup3r-secret@api.poiskkino.dev")]
    [InlineData("https://admin:sup3r-secret@eu-api.poiskkino.dev:8443")]
    public void Constructor_UserInfoInBaseUrl_RedactedInWarning(string rawApiBaseUrl)
    {
        const string secret = "sup3r-secret";

        _ = CreateClientWith(rawApiBaseUrl);

        Assert.Contains(secret, rawApiBaseUrl, StringComparison.Ordinal); // контроль тестовых данных: пароль во входе есть
        VerifyLog(LogLevel.Warning, Times.Once());
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) =>
                    !state.ToString()!.Contains(secret, StringComparison.Ordinal) &&
                    state.ToString()!.Contains("poiskkino.dev", StringComparison.Ordinal)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // AC-9: API-ключ не попадает ни в одно сообщение лога клиента.
    [Fact]
    public async Task ApiKey_NeverAppearsInLogs()
    {
        var client = CreateClientWith("http://evil.example");

        await client.SearchAsync("Secret Query", null, TestApiKey);
        await client.GetMovieByIdAsync(535341, TestApiKey);
        await client.GetSeasonAsync(535341, 1, TestApiKey);

        _loggerMock.Verify(
            l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains(TestApiKey, StringComparison.Ordinal)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    private PoiskKinoApiClient CreateClient()
    {
        var factoryMock = CreateHttpClientFactoryMock();
        return new PoiskKinoApiClient(factoryMock.Object, _loggerMock.Object);
    }

    private PoiskKinoApiClient CreateClientWith(string rawApiBaseUrl)
    {
        var factoryMock = CreateHttpClientFactoryMock();
        return new PoiskKinoApiClient(factoryMock.Object, _loggerMock.Object, rawApiBaseUrl);
    }

    private Mock<IHttpClientFactory> CreateHttpClientFactoryMock()
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => _capturedRequests.Add(request))
            .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(EmptySearchResponseJson, new MediaTypeHeaderValue("application/json")),
            });

        // Клиент сам ставит BaseAddress, Timeout и User-Agent — фабрика отдаёт HttpClient без BaseAddress.
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(handlerMock.Object));
        return factoryMock;
    }

    private void VerifyLog(LogLevel level, Times times) =>
        _loggerMock.Verify(
            l => l.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, _) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);

    private void VerifyLoggedMessageContains(LogLevel level, string expectedSubstring) =>
        _loggerMock.Verify(
            l => l.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains(expectedSubstring, StringComparison.Ordinal)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
}
