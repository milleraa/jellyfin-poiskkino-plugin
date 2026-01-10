using System.Net.Http;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Jellyfin.Data.Enums;

namespace PoiskKinoMetadataPlugin;

/// <summary>
/// Провайдер метаданных для фильмов из PoiskKino API.
/// </summary>
/// <remarks>
/// Создаёт новый экземпляр класса <see cref="PoiskKinoMovieProvider"/>.
/// </remarks>
public class PoiskKinoMovieProvider(
    IHttpClientFactory httpClientFactory,
    ILoggerFactory loggerFactory,
    ILogger<PoiskKinoMovieProvider> logger) : IRemoteMetadataProvider<Movie, MovieInfo>, IHasOrder
{
    private readonly Lazy<PoiskKinoApiClient> _apiClient = new Lazy<PoiskKinoApiClient>(() => 
        new PoiskKinoApiClient(httpClientFactory, loggerFactory.CreateLogger<PoiskKinoApiClient>()));
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<PoiskKinoMovieProvider> _logger = logger;
    
    private PoiskKinoApiClient ApiClient => _apiClient.Value;

    private string ApiKey => Plugin.Instance?.Configuration?.ApiKey ?? string.Empty;

    /// <inheritdoc />
    public string Name => "ПоискКино";

    /// <inheritdoc />
    public int Order => 0;

    /// <inheritdoc />
    public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(
        MovieInfo searchInfo,
        CancellationToken cancellationToken)
    {
        return GetSearchResultsInternal(searchInfo, cancellationToken);
    }

    /// <inheritdoc />
    public Task<MetadataResult<Movie>> GetMetadata(
        MovieInfo info,
        CancellationToken cancellationToken)
    {
        return GetMetadataInternal(info, cancellationToken);
    }

    private async Task<IEnumerable<RemoteSearchResult>> GetSearchResultsInternal(
        MovieInfo searchInfo,
        CancellationToken cancellationToken)
    {
        var results = new List<RemoteSearchResult>();

        try
        {
            var title = searchInfo.Name;
            if (string.IsNullOrWhiteSpace(title))
            {
                return results;
            }

            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                return results;
            }

            var searchResponse = await ApiClient.SearchAsync(
                title,
                searchInfo.Year,
                ApiKey,
                cancellationToken);

            if (searchResponse?.Results == null)
            {
                return results;
            }

            // Фильтруем не-сериальный контент (фильмы, мультфильмы, аниме и т.д.)
            var movies = searchResponse.Results
                .Where(r => r.IsSeries != true)
                .ToList();

            foreach (var item in movies)
            {
                var searchResult = new RemoteSearchResult
                {
                    Name = item.Name ?? item.EnName,
                    ProductionYear = item.Year,
                    Overview = item.Description,
                    ProviderIds = []
                };

                searchResult.ProviderIds[ProviderNames.PoiskKino] = item.Id.ToString();

                if (!string.IsNullOrEmpty(item.ExternalId?.Imdb))
                {
                    searchResult.ProviderIds[MetadataProvider.Imdb.ToString()] = item.ExternalId.Imdb;
                }

                if (!string.IsNullOrEmpty(item.ExternalId?.KpHd))
                {
                    searchResult.ProviderIds["Kinopoisk"] = item.ExternalId.KpHd;
                }

                if (!string.IsNullOrEmpty(item.ExternalId?.Tmdb.ToString()))
                {
                    searchResult.ProviderIds[MetadataProvider.Tmdb.ToString()] = item.ExternalId.Tmdb.ToString();
                }

                if (!string.IsNullOrEmpty(item.Poster?.Url) && !ImageUrlHelper.ShouldFilterUrl(item.Poster.Url))
                {
                    searchResult.ImageUrl = item.Poster.Url;
                }

                results.Add(searchResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting search results for {Title}", searchInfo.Name);
        }

        return results;
    }

    private async Task<MetadataResult<Movie>> GetMetadataInternal(
        MovieInfo info,
        CancellationToken cancellationToken)
    {
        var result = new MetadataResult<Movie>();

        try
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                return result;
            }

            Models.PoiskKinoMovieDtoV1_4? movieData = null;

            // Сначала проверяем, есть ли PoiskKino ID в ProviderIds
            var poiskKinoIdStr = info.GetProviderId(ProviderNames.PoiskKino);
            if (!string.IsNullOrEmpty(poiskKinoIdStr) && int.TryParse(poiskKinoIdStr, out var poiskKinoId))
            {
                // Если есть ID, получаем детальную информацию напрямую
                try
                {
                    movieData = await ApiClient.GetMovieByIdAsync(poiskKinoId, ApiKey, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get movie data by ID {Id}, falling back to search", poiskKinoId);
                }
            }

            // Если не получили данные по ID, используем поиск
            if (movieData == null)
            {
                var searchResults = await GetSearchResultsInternal(info, cancellationToken);
                var firstResult = searchResults.FirstOrDefault();

                if (firstResult != null)
                {
                    var poiskKinoIdFromSearch = firstResult.GetProviderId(ProviderNames.PoiskKino);
                    if (!string.IsNullOrEmpty(poiskKinoIdFromSearch) && int.TryParse(poiskKinoIdFromSearch, out var poiskKinoIdFromSearchInt))
                    {
                        // Получаем полные данные фильма по ID из результата поиска
                        try
                        {
                            movieData = await ApiClient.GetMovieByIdAsync(poiskKinoIdFromSearchInt, ApiKey, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to get full movie data for ID {Id} from search result", poiskKinoIdFromSearchInt);
                        }
                    }
                }
                }

            var itemTitle = info.Name ?? string.Empty;
            var movie = new Movie
            {
                Name = movieData != null
                    ? (movieData.Name ?? movieData.EnName ?? itemTitle)
                    : itemTitle,
                Overview = movieData != null
                    ? (movieData.Description ?? movieData.ShortDescription)
                    : null,
                ProductionYear = movieData?.Year ?? info.Year,
                ProviderIds = info.ProviderIds != null 
                    ? new Dictionary<string, string>(info.ProviderIds) 
                    : new Dictionary<string, string>()
            };
            result.Item = movie;

            // Set external IDs - сохраняем все 4 провайдера из детальных данных
            if (movieData != null)
            {
                // 1. PoiskKino ID
                result.Item.ProviderIds[ProviderNames.PoiskKino] = movieData.Id.ToString();

                // Сохраняем все externalId если есть
                if (movieData.ExternalId != null)
                {
                    // 2. Kinopoisk (kpHD)
                    if (!string.IsNullOrEmpty(movieData.ExternalId.KpHd))
                    {
                        result.Item.ProviderIds["Kinopoisk"] = movieData.ExternalId.KpHd;
                    }
                    
                    // 3. IMDb
                    if (!string.IsNullOrEmpty(movieData.ExternalId.Imdb))
                    {
                        result.Item.ProviderIds[MetadataProvider.Imdb.ToString()] = movieData.ExternalId.Imdb;
                    }
                    
                    // 4. TMDB
                    if (movieData.ExternalId.Tmdb.HasValue)
                    {
                        result.Item.ProviderIds[MetadataProvider.Tmdb.ToString()] = movieData.ExternalId.Tmdb.Value.ToString();
                    }
                }
            }

            // Устанавливаем жанры
            if (movieData != null && movieData.Genres != null && movieData.Genres.Count > 0)
            {
                result.Item.Genres = movieData.Genres
                    .Where(g => !string.IsNullOrEmpty(g.Name))
                    .Select(g => g.Name!)
                    .ToArray();
            }

            // Устанавливаем рейтинги
            if (movieData != null && movieData.Rating != null)
            {
                if (movieData.Rating.Imdb.HasValue)
                {
                    result.Item.CommunityRating = (float)movieData.Rating.Imdb.Value;
                }
                if (movieData.Rating.Kinopoisk.HasValue)
                {
                    result.Item.CriticRating = (float)movieData.Rating.Kinopoisk.Value;
                }
            }

            // Устанавливаем персоналии (актёры, режиссёры и т.д.)
            if (movieData != null && movieData.Persons != null && movieData.Persons.Count > 0)
            {
                foreach (var person in movieData.Persons)
                {
                    var personInfo = new PersonInfo
                    {
                        Name = person.Name ?? person.EnName ?? string.Empty,
                        Type = GetPersonKind(person.Profession ?? person.EnProfession)
                    };

                    if (!string.IsNullOrEmpty(person.Photo) && !ImageUrlHelper.ShouldFilterUrl(person.Photo))
                    {
                        personInfo.ImageUrl = person.Photo;
                    }

                    result.AddPerson(personInfo);
                }
            }

            result.HasMetadata = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting metadata for {Title}", info.Name);
        }

        return result;
    }


    /// <summary>
    /// Определяет тип персоны по строке профессии.
    /// </summary>
    private static PersonKind GetPersonKind(string? profession)
    {
        if (string.IsNullOrEmpty(profession))
        {
            return PersonKind.Actor;
        }

        var profLower = profession.ToLowerInvariant();
        if (profLower.Contains("режиссер") || profLower.Contains("director"))
        {
            return PersonKind.Director;
        }
        if (profLower.Contains("актер") || profLower.Contains("актриса") || profLower.Contains("actor") || profLower.Contains("actress"))
        {
            return PersonKind.Actor;
        }
        if (profLower.Contains("продюсер") || profLower.Contains("producer"))
        {
            return PersonKind.Producer;
        }
        if (profLower.Contains("сценарист") || profLower.Contains("writer") || profLower.Contains("screenplay"))
        {
            return PersonKind.Writer;
        }
        if (profLower.Contains("композитор") || profLower.Contains("composer"))
        {
            return PersonKind.Composer;
        }

        return PersonKind.Actor;
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

