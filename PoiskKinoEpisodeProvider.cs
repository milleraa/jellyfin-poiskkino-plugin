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
/// Провайдер метаданных для эпизодов сериалов из PoiskKino API.
/// </summary>
public class PoiskKinoEpisodeProvider(
    IHttpClientFactory httpClientFactory,
    ILoggerFactory loggerFactory,
    ILogger<PoiskKinoEpisodeProvider> logger) : IRemoteMetadataProvider<Episode, EpisodeInfo>, IHasOrder
{
    private readonly Lazy<PoiskKinoApiClient> _apiClient = new Lazy<PoiskKinoApiClient>(() => 
        new PoiskKinoApiClient(httpClientFactory, loggerFactory.CreateLogger<PoiskKinoApiClient>()));
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<PoiskKinoEpisodeProvider> _logger = logger;
    
    private PoiskKinoApiClient ApiClient => _apiClient.Value;

    private string ApiKey => Plugin.Instance?.Configuration?.ApiKey ?? string.Empty;

    /// <inheritdoc />
    public string Name => "ПоискКино";

    /// <inheritdoc />
    public int Order => 0;

    /// <inheritdoc />
    public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(
        EpisodeInfo searchInfo,
        CancellationToken cancellationToken)
    {
        // Эпизоды обычно не ищутся напрямую
        return Task.FromResult<IEnumerable<RemoteSearchResult>>(Array.Empty<RemoteSearchResult>());
    }

    /// <inheritdoc />
    public Task<MetadataResult<Episode>> GetMetadata(
        EpisodeInfo info,
        CancellationToken cancellationToken)
    {
        return GetMetadataInternal(info, cancellationToken);
    }

    private async Task<MetadataResult<Episode>> GetMetadataInternal(
        EpisodeInfo info,
        CancellationToken cancellationToken)
    {
        var result = new MetadataResult<Episode>();

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
                _logger.LogDebug("Cannot determine series ID for episode S{SeasonNumber}E{EpisodeNumber}",
                    info.ParentIndexNumber, info.IndexNumber);
                return result;
            }

            var seasonNumber = info.ParentIndexNumber ?? 1;
            var episodeNumber = info.IndexNumber ?? 1;

            // Получаем данные сезона с эпизодами
            var seasonData = await ApiClient.GetSeasonAsync(
                seriesId.Value,
                seasonNumber,
                ApiKey,
                cancellationToken);

            if (seasonData?.Episodes == null || seasonData.Episodes.Count == 0)
            {
                return result;
            }

            // Находим конкретный эпизод
            var episode = seasonData.Episodes
                .FirstOrDefault(e => e.Number == episodeNumber);

            if (episode == null)
            {
                return result;
            }

            result.Item = new Episode
            {
                Name = episode.Name ?? episode.EnName ?? $"Episode {episodeNumber}",
                Overview = episode.Description ?? episode.EnDescription,
                IndexNumber = episode.Number,
                ParentIndexNumber = seasonNumber,
                PremiereDate = !string.IsNullOrEmpty(episode.AirDate) && DateTime.TryParse(episode.AirDate, out var airDate) 
                    ? airDate 
                    : episode.Date,
                ProviderIds = info.ProviderIds != null 
                    ? new Dictionary<string, string>(info.ProviderIds) 
                    : new Dictionary<string, string>()
            };

            // Устанавливаем изображение эпизода, если доступно
            if (episode.Still != null && !string.IsNullOrEmpty(episode.Still.Url) && !ImageUrlHelper.ShouldFilterUrl(episode.Still.Url))
            {
                result.Item.AddImage(new ItemImageInfo
                {
                    Path = episode.Still.Url,
                    Type = ImageType.Primary
                });
            }

            result.HasMetadata = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting metadata for episode S{SeasonNumber}E{EpisodeNumber}", 
                info.ParentIndexNumber, info.IndexNumber);
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

