using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace PoiskKinoMetadataPlugin.UnitTests.ApiClient;

/// <summary>
/// Тесты параметра лимита поиска в <see cref="PoiskKinoApiClient"/>: URL, валидация, Warning и его
/// дедупликация (AC-2, AC-3, AC-4, AC-6 задачи 002).
/// </summary>
/// <remarks>
/// Фактический URL проверяется по захваченному <see cref="HttpRequestMessage.RequestUri"/>,
/// лог — через Moq-логгер <see cref="PluginTestFixture.CreateLoggerMock{T}"/>.
/// Кэш-аспекты (AC-5, AC-8) — в <see cref="PoiskKinoApiClientTests"/>, рядом с существующими кэш-тестами.
/// </remarks>
public class PoiskKinoApiClientSearchLimitTests
{
    private const string TestApiKey = "test-api-key";
    private const string EmptySearchResponseJson = "{\"docs\":[],\"total\":0}";

    private readonly List<HttpRequestMessage> _capturedRequests = [];
    private readonly Mock<ILogger<PoiskKinoApiClient>> _loggerMock = PluginTestFixture.CreateLoggerMock<PoiskKinoApiClient>();

    // AC-3: дефолтное значение даёт запрос, идентичный историческому (limit=3), без Warning.
    [Fact]
    public async Task SearchAsync_DefaultLimit_RequestsLimit3()
    {
        var client = CreateClient();

        var exception = await Record.ExceptionAsync(() => client.SearchAsync("Test", null, TestApiKey));

        Assert.Null(exception);
        var request = Assert.Single(_capturedRequests);
        Assert.Equal("/v1.5/movie/search", request.RequestUri!.AbsolutePath);
        Assert.Equal("?query=Test&limit=3", request.RequestUri.Query);
        VerifyLog(LogLevel.Warning, Times.Never());
    }

    // AC-2: явное значение (именованный аргумент) попадает в URL limit=N без Warning.
    [Theory]
    [InlineData(1, "?query=Test&limit=1")]
    [InlineData(7, "?query=Test&limit=7")]
    [InlineData(10, "?query=Test&limit=10")]
    public async Task SearchAsync_ExplicitSearchLimit_RequestsConfiguredLimit(int searchLimit, string expectedQuery)
    {
        var client = CreateClient();

        await client.SearchAsync("Test", null, TestApiKey, searchLimit: searchLimit);

        var request = Assert.Single(_capturedRequests);
        Assert.Equal(expectedQuery, request.RequestUri!.Query);
        VerifyLog(LogLevel.Warning, Times.Never());
    }

    // AC-4/AC-6: невалидное значение — дефолт limit=3, ровно один Warning, исключений нет.
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(11)]
    public async Task SearchAsync_InvalidSearchLimit_RequestsDefaultLimitWithSingleWarning(int searchLimit)
    {
        var client = CreateClient();

        var exception = await Record.ExceptionAsync(
            () => client.SearchAsync("Test", null, TestApiKey, searchLimit: searchLimit));

        Assert.Null(exception);
        var request = Assert.Single(_capturedRequests);
        Assert.Equal("?query=Test&limit=3", request.RequestUri!.Query);
        VerifyLog(LogLevel.Warning, Times.Once());
    }

    // AC-4: Warning содержит сырое значение и причину fallback, но не API-ключ.
    [Fact]
    public async Task SearchAsync_InvalidSearchLimit_WarningContainsRawValueAndReasonWithoutApiKey()
    {
        var client = CreateClient();

        await client.SearchAsync("Secret Query", null, TestApiKey, searchLimit: 0);

        VerifyLog(LogLevel.Warning, Times.Once());
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) =>
                    state.ToString()!.Contains("Invalid search limit 0", StringComparison.Ordinal) &&
                    state.ToString()!.Contains("falling back to default 3", StringComparison.Ordinal) &&
                    !state.ToString()!.Contains(TestApiKey, StringComparison.Ordinal)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // AC-4 (A1): повторный поиск с тем же невалидным значением не дублирует Warning.
    [Fact]
    public async Task SearchAsync_RepeatedInvalidSearchLimit_WarnsOnce()
    {
        var client = CreateClient();

        await client.SearchAsync("Test", null, TestApiKey, searchLimit: 0);
        await client.SearchAsync("Test", null, TestApiKey, searchLimit: 0);

        VerifyLog(LogLevel.Warning, Times.Once());
    }

    // AC-4 (A1): другое невалидное значение даёт новое предупреждение.
    [Fact]
    public async Task SearchAsync_ChangedInvalidSearchLimit_WarnsAgain()
    {
        var client = CreateClient();

        await client.SearchAsync("Test", null, TestApiKey, searchLimit: 0);
        await client.SearchAsync("Test", null, TestApiKey, searchLimit: -1);

        VerifyLog(LogLevel.Warning, Times.Exactly(2));
    }

    // AC-4 (A1): валидное значение сбрасывает памятку — эпизод «исправил и снова сломал» предупреждает заново.
    [Fact]
    public async Task SearchAsync_ValidSearchLimitBetweenInvalidOnes_ResetsWarningMemo()
    {
        var client = CreateClient();

        await client.SearchAsync("Test", null, TestApiKey, searchLimit: 0);
        await client.SearchAsync("Test", null, TestApiKey, searchLimit: 7);
        await client.SearchAsync("Test", null, TestApiKey, searchLimit: 0);

        VerifyLog(LogLevel.Warning, Times.Exactly(2));
    }

    private PoiskKinoApiClient CreateClient()
    {
        var factoryMock = CreateHttpClientFactoryMock();
        return new PoiskKinoApiClient(factoryMock.Object, _loggerMock.Object);
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
}
