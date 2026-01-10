using System.Net.Http;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace PoiskKinoMetadataPlugin;

/// <summary>
/// Провайдер изображений (постеры и фоны) из PoiskKino API.
/// </summary>
/// <remarks>
/// Создаёт новый экземпляр класса <see cref="PoiskKinoImageProvider"/>.
/// </remarks>
public class PoiskKinoImageProvider(
    IHttpClientFactory httpClientFactory,
    ILoggerFactory loggerFactory,
    ILogger<PoiskKinoImageProvider> logger) : IRemoteImageProvider, IHasOrder
{
    private readonly Lazy<PoiskKinoApiClient> _apiClient = new Lazy<PoiskKinoApiClient>(() => 
        new PoiskKinoApiClient(httpClientFactory, loggerFactory.CreateLogger<PoiskKinoApiClient>()));
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<PoiskKinoImageProvider> _logger = logger;
    
    private PoiskKinoApiClient ApiClient => _apiClient.Value;

    private string ApiKey => Plugin.Instance?.Configuration?.ApiKey ?? string.Empty;

    /// <inheritdoc />
    public string Name => "ПоискКино";

    /// <inheritdoc />
    public int Order => 0;

    /// <inheritdoc />
    public bool Supports(BaseItem item)
    {
        return item is Series || item is Season || item is Movie;
    }

    /// <inheritdoc />
    public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
    {
        return [ImageType.Primary, ImageType.Backdrop];
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RemoteImageInfo>> GetImages(
        BaseItem item,
        CancellationToken cancellationToken)
    {
        var images = new List<RemoteImageInfo>();

        try
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                return images;
            }
            
            // Сначала пытаемся получить ID из ProviderIds - используем PoiskKino ID
            int? itemId = null;
            var poiskKinoId = item.GetProviderId(ProviderNames.PoiskKino);
            if (!string.IsNullOrEmpty(poiskKinoId) && int.TryParse(poiskKinoId, out var poiskKinoIdInt))
            {
                itemId = poiskKinoIdInt;
            }

            Models.PoiskKinoMovieDtoV1_4? movieData = null;

            // Если есть ID, используем GetMovieByIdAsync для получения полных данных
            if (itemId.HasValue)
            {
                movieData = await ApiClient.GetMovieByIdAsync(itemId.Value, ApiKey, cancellationToken);
            }

            // Если нет ID или GetMovieByIdAsync не удался, используем поиск
            // Пропускаем поиск для сезонов - искать "Сезон N" бессмысленно
            if (movieData == null && item is not Season)
            {
                var title = item.Name;
                if (string.IsNullOrWhiteSpace(title))
                {
                    return images;
                }

                var searchResponse = await ApiClient.SearchAsync(
                    title,
                    item.ProductionYear,
                    ApiKey,
                    cancellationToken);

                if (searchResponse?.Results == null || searchResponse.Results.Count == 0)
                {
                    return images;
                }

                // Находим подходящий элемент - фильтруем по флагу IsSeries
                var isSeries = item is Series;
                var apiItem = searchResponse.Results
                    .Where(r => isSeries ? r.IsSeries == true : r.IsSeries != true)
                    .OrderByDescending(r => r.Year == item.ProductionYear ? 1 : 0)
                    .ThenByDescending(r =>
                    {
                        // Пытаемся найти совпадение по названию
                        var itemTitleLower = item.Name?.ToLowerInvariant() ?? string.Empty;
                        var resultTitleLower = (r.Name ?? r.EnName ?? string.Empty).ToLowerInvariant();
                        return itemTitleLower == resultTitleLower ? 1 : 0;
                    })
                    .FirstOrDefault();

                if (apiItem == null)
                {
                    return images;
                }

                // Пытаемся получить полные данные по ID из результата поиска
                if (apiItem.Id > 0)
                {
                    movieData = await ApiClient.GetMovieByIdAsync(apiItem.Id, ApiKey, cancellationToken);
                }

                // Используем данные из результата поиска, если полные данные недоступны
                if (movieData == null)
                {
                    // Добавляем постер из результата поиска
                    if (!string.IsNullOrEmpty(apiItem.Poster?.Url))
                    {
                        images.Add(new RemoteImageInfo
                        {
                            Url = apiItem.Poster.Url,
                            Type = ImageType.Primary,
                            ProviderName = Name
                        });
                    }

                    // Добавляем фон из результата поиска
                    if (!string.IsNullOrEmpty(apiItem.Backdrop?.Url))
                    {
                        images.Add(new RemoteImageInfo
                        {
                            Url = apiItem.Backdrop.Url,
                            Type = ImageType.Backdrop,
                            ProviderName = Name
                        });
                    }

                    return images;
                }
            }

            // Данные недоступны
            if (movieData == null)
            {
                return images;
            }

            // Используем полные данные фильма
            if (movieData.Poster != null && !string.IsNullOrEmpty(movieData.Poster.Url))
            {
                images.Add(new RemoteImageInfo
                {
                    Url = movieData.Poster.Url,
                    Type = ImageType.Primary,
                    ProviderName = Name
                });
            }

            if (movieData.Backdrop != null && !string.IsNullOrEmpty(movieData.Backdrop.Url))
            {
                images.Add(new RemoteImageInfo
                {
                    Url = movieData.Backdrop.Url,
                    Type = ImageType.Backdrop,
                    ProviderName = Name
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting images for {Title}", item.Name);
        }

        // Фильтруем изображения TMDB, если это настроено
        if (ImageUrlHelper.ShouldIgnoreTmdbImages())
        {
            var filteredImages = images.Where(img => !ImageUrlHelper.IsTmdbUrl(img.Url)).ToList();
            if (filteredImages.Count < images.Count)
            {
                _logger.LogDebug("Filtered out {Count} TMDB images for {Title}", images.Count - filteredImages.Count, item.Name);
            }
            return filteredImages;
        }

        return images;
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

