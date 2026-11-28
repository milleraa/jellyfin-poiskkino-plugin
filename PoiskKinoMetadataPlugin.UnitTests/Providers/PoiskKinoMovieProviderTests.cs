using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    public async Task GetSearchResults_YearFilter_MatchesExactYear()
    {
        PluginTestFixture.SetUpPlugin(TestApiKey);
        SetupSearchResponse(HttpStatusCode.OK, TestJsonData.MixedSearchResponseJson);
        var provider = CreateProvider();
        var info = new MovieInfo { Name = "Test", Year = 2023 };

        var results = (await provider.GetSearchResults(info, CancellationToken.None)).ToList();

        Assert.Single(results);
        Assert.Equal("535341", results[0].GetProviderId(ProviderNames.PoiskKino));
    }

    [Fact]
    public async Task GetSearchResults_YearFilter_NoMatch_FallsBackToUnfiltered()
    {
        PluginTestFixture.SetUpPlugin(TestApiKey);
        SetupSearchResponse(HttpStatusCode.OK, TestJsonData.SearchResponseJson);
        var provider = CreateProvider();
        var info = new MovieInfo { Name = "Оппенгеймер", Year = 1999 };

        var results = (await provider.GetSearchResults(info, CancellationToken.None)).ToList();

        Assert.Single(results);
        Assert.Equal("535341", results[0].GetProviderId(ProviderNames.PoiskKino));
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
    [InlineData("режиссер", null, PersonKind.Director)]
    [InlineData("Director", null, PersonKind.Director)]
    [InlineData("director", null, PersonKind.Director)]
    [InlineData("актер", null, PersonKind.Actor)]
    [InlineData("актриса", null, PersonKind.Actor)]
    [InlineData("Actor", null, PersonKind.Actor)]
    [InlineData("actress", null, PersonKind.Actor)]
    [InlineData("продюсер", null, PersonKind.Producer)]
    [InlineData("Producer", null, PersonKind.Producer)]
    [InlineData("сценарист", null, PersonKind.Writer)]
    [InlineData("Writer", null, PersonKind.Writer)]
    [InlineData("Screenplay writer", null, PersonKind.Writer)]
    [InlineData("композитор", null, PersonKind.Composer)]
    [InlineData("Composer", null, PersonKind.Composer)]
    [InlineData("unknown role", null, PersonKind.Actor)]
    [InlineData("", null, PersonKind.Actor)]
    [InlineData(null, null, PersonKind.Actor)]
    [InlineData(null, "Director", PersonKind.Director)]
    public async Task GetMetadata_MapsPersonRole(string? profession, string? enProfession, PersonKind expected)
    {
        var json = JsonSerializer.Serialize(
            new { id = 123, persons = new[] { new { name = "Test Person", profession, enProfession } } },
            new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
        SetupMovieByIdResponse(HttpStatusCode.OK, json);
        var provider = CreateProvider();
        var info = new MovieInfo();
        info.SetProviderId(ProviderNames.PoiskKino, "123");

        var result = await provider.GetMetadata(info, CancellationToken.None);

        var person = Assert.Single(result.People);
        Assert.Equal(expected, person.Type);
    }

    [Fact]
    public async Task GetSearchResults_CustomApiBaseUrl_RequestGoesToConfiguredHost()
    {
        PluginTestFixture.SetUpPlugin(TestApiKey);
        var capturedRequests = new List<HttpRequestMessage>();
        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => capturedRequests.Add(request))
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    TestJsonData.SearchResponseJson,
                    new MediaTypeHeaderValue("application/json")),
            });

        try
        {
            // AC-5 (проводка): значение из конфигурации доходит до фактического адреса запроса.
            Plugin.Instance!.Configuration.ApiBaseUrl = "https://eu-api.poiskkino.dev";
            var provider = CreateProvider();

            await provider.GetSearchResults(new MovieInfo { Name = "Оппенгеймер" }, CancellationToken.None);

            var request = Assert.Single(capturedRequests);
            Assert.Equal("https", request.RequestUri!.Scheme);
            Assert.Equal("eu-api.poiskkino.dev", request.RequestUri.Host);
            Assert.Equal("/v1.5/movie/search", request.RequestUri.AbsolutePath);
        }
        finally
        {
            // Пересоздаём плагин с дефолтом, чтобы значение не протекало в другие тесты коллекции.
            PluginTestFixture.SetUpPlugin(TestApiKey);
        }
    }

    [Fact]
    public async Task GetSearchResults_SearchLimitFromConfiguration_ChangesRequestLimitWithoutRestart()
    {
        PluginTestFixture.SetUpPlugin(TestApiKey);
        var capturedRequests = new List<HttpRequestMessage>();
        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => capturedRequests.Add(request))
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    TestJsonData.SearchResponseJson,
                    new MediaTypeHeaderValue("application/json")),
            });

        try
        {
            // AC-7: лимит читается из конфигурации при каждом вызове поиска (паттерн ApiKey),
            // а не фиксируется при создании провайдера/клиента: смена значения между двумя
            // поисками меняет URL второго запроса без пересоздания провайдера и клиента.
            Plugin.Instance!.Configuration.SearchLimit = 7;
            var provider = CreateProvider();

            await provider.GetSearchResults(new MovieInfo { Name = "Оппенгеймер" }, CancellationToken.None);
            Plugin.Instance.Configuration.SearchLimit = 4;
            await provider.GetSearchResults(new MovieInfo { Name = "Оппенгеймер" }, CancellationToken.None);

            Assert.Equal(2, capturedRequests.Count);
            Assert.Contains("limit=7", capturedRequests[0].RequestUri!.Query);
            Assert.Contains("limit=4", capturedRequests[1].RequestUri!.Query);
        }
        finally
        {
            // Пересоздаём плагин с дефолтом, чтобы значение не протекало в другие тесты коллекции.
            PluginTestFixture.SetUpPlugin(TestApiKey);
        }
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