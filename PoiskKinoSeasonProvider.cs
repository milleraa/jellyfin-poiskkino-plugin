using System.Net.Http;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace PoiskKinoMetadataPlugin;

/// <summary>
/// Провайдер метаданных для сезонов сериалов из PoiskKino API.
/// </summary>
public class PoiskKinoSeasonProvider(
    IHttpClientFactory httpClientFactory,
    ILoggerFactory loggerFactory,
    ILogger<PoiskKinoSeasonProvider> logger) : IRemoteMetadataProvider<Season, SeasonInfo>, IHasOrder
{
    private readonly Lazy<PoiskKinoApiClient> _apiClient = new Lazy<PoiskKinoApiClient>(() => 
        new PoiskKinoApiClient(httpClientFactory, loggerFactory.CreateLogger<PoiskKinoApiClient>()));
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<PoiskKinoSeasonProvider> _logger = logger;
    
    private PoiskKinoApiClient ApiClient => _apiClient.Value;

    private string ApiKey => Plugin.Instance?.Configuration?.ApiKey ?? string.Empty;

    /// <inheritdoc />
    public string Name => "ПоискКино";

    /// <inheritdoc />
    public int Order => 0;

    /// <inheritdoc />
    public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(
        SeasonInfo searchInfo,
        CancellationToken cancellationToken)
    {
        // Сезоны обычно не ищутся напрямую
        return Task.FromResult<IEnumerable<RemoteSearchResult>>(Array.Empty<RemoteSearchResult>());
    }

    /// <inheritdoc />
    public Task<MetadataResult<Season>> GetMetadata(
        SeasonInfo info,
        CancellationToken cancellationToken)
    {
        return GetMetadataInternal(info, cancellationToken);
    }

    private async Task<MetadataResult<Season>> GetMetadataInternal(
        SeasonInfo info,
        CancellationToken cancellationToken)
    {
        var result = new MetadataResult<Season>();

        try
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                return result;
            }

            // Пытаемся получить ID сериала из ProviderIds - используем PoiskKino ID
            int? seriesId = null;
            if (info.SeriesProviderIds != null && info.SeriesProviderIds.TryGetValue(ProviderNames.PoiskKino, out var poiskKinoId) && int.TryParse(poiskKinoId, out var poiskKinoIdInt))
            {
                seriesId = poiskKinoIdInt;
            }

            if (!seriesId.HasValue)
            {
                _logger.LogDebug("Cannot determine series ID for season S{SeasonNumber}", 
                    info.IndexNumber);
                return result;
            }

            var seasonNumber = info.IndexNumber ?? 1;

            // Получаем данные сезона из PoiskKino API
            var seasonData = await ApiClient.GetSeasonAsync(
                seriesId.Value,
                seasonNumber,
                ApiKey,
                cancellationToken);

            if (seasonData == null)
            {
                return result;
            }

            result.Item = new Season
            {
                Name = seasonData.Name ?? seasonData.EnName ?? $"Season {seasonNumber}",
                Overview = seasonData.Description ?? seasonData.EnDescription,
                IndexNumber = seasonData.Number,
                PremiereDate = !string.IsNullOrEmpty(seasonData.AirDate) && DateTime.TryParse(seasonData.AirDate, out var airDate) 
                    ? airDate 
                    : null,
                ProviderIds = info.ProviderIds != null 
                    ? new Dictionary<string, string>(info.ProviderIds) 
                    : new Dictionary<string, string>()
            };

            // Устанавливаем изображение сезона, если доступно
            if (seasonData.Poster != null && !string.IsNullOrEmpty(seasonData.Poster.Url) && !ImageUrlHelper.ShouldFilterUrl(seasonData.Poster.Url))
            {
                result.Item.AddImage(new ItemImageInfo
                {
                    Path = seasonData.Poster.Url,
                    Type = ImageType.Primary
                });
            }

            result.HasMetadata = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting metadata for season S{SeasonNumber}", 
                info.IndexNumber);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
        }

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync(url, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading image from {Url}", url);
            return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
        }
    }
}

