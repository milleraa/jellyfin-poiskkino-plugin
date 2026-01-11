using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using PoiskKinoMetadataPlugin.Models;

namespace PoiskKinoMetadataPlugin;

/// <summary>
/// HTTP-клиент для PoiskKino API с поддержкой кэширования и ограничения частоты запросов.
/// </summary>
public class PoiskKinoApiClient
{
    private const string ApiBaseUrl = "https://api.poiskkino.dev";
    private const int CacheExpirationHours = 24;
    private const int MaxRequestsPerDay = 200;

    private readonly HttpClient _httpClient;
    private readonly ILogger<PoiskKinoApiClient> _logger;
    private readonly ConcurrentDictionary<string, CacheEntry<object>> _cache;
    private readonly SemaphoreSlim _requestSemaphore;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="PoiskKinoApiClient"/>.
    /// </summary>
    /// <param name="httpClientFactory">Фабрика HTTP-клиентов.</param>
    /// <param name="logger">Экземпляр логгера.</param>
    public PoiskKinoApiClient(IHttpClientFactory httpClientFactory, ILogger<PoiskKinoApiClient> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(ApiBaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(120);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Jellyfin-PoiskKino-Plugin/1.0");
        
        _logger = logger;
        _cache = new ConcurrentDictionary<string, CacheEntry<object>>();
        _requestSemaphore = new SemaphoreSlim(1, 1);
    }

    /// <summary>
    /// Выполняет поиск фильмов или сериалов по названию и году.
    /// </summary>
    /// <param name="title">Название для поиска.</param>
    /// <param name="year">Год выпуска (опционально).</param>
    /// <param name="apiKey">API-ключ для аутентификации.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Ответ с результатами поиска или null, если не найдено или произошла ошибка.</returns>
    public async Task<PoiskKinoSearchResponse?> SearchAsync(
        string title,
        int? year,
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("API key is not configured");
            return null;
        }

        var cacheKey = GenerateCacheKey(title, year);
        
        // Сначала проверяем кэш
        if (_cache.TryGetValue(cacheKey, out var cachedEntry))
        {
            if (cachedEntry.ExpiresAt > DateTime.UtcNow)
            {
                _logger.LogDebug("Cache hit for {Title} ({Year})", title, year);
                return cachedEntry.Data as PoiskKinoSearchResponse;
            }
            
            // Удаляем устаревшую запись
            _cache.TryRemove(cacheKey, out _);
        }

        // Выполняем API-запрос
        try
        {
            await _requestSemaphore.WaitAsync(cancellationToken);
            
            var url = $"/v1.4/movie/search?query={Uri.EscapeDataString(title)}&limit=3";
            if (year.HasValue)
            {
                url += $"&year={year.Value}";
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-API-KEY", apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                var errorMessage = await GetErrorMessageAsync(response, cancellationToken);
                _logger.LogWarning("API limit exceeded ({StatusCode}) for {Title}: {Message}", (int)response.StatusCode, title, errorMessage);
                return null;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogDebug("Not found (404) for {Title} ({Year})", title, year);
                // Кэшируем отрицательный результат на меньшее время
                _cache.TryAdd(cacheKey, new CacheEntry<object>(null, DateTime.UtcNow.AddHours(1)));
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("API request failed with status {StatusCode} for {Title}", response.StatusCode, title);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var searchResponse = JsonSerializer.Deserialize<PoiskKinoSearchResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Кэшируем успешный результат
            if (searchResponse != null)
            {
                _cache.TryAdd(cacheKey, new CacheEntry<object>(searchResponse, DateTime.UtcNow.AddHours(CacheExpirationHours)));
            }

            return searchResponse;
        }
        catch (TaskCanceledException)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Request cancelled by user for {Title}", title);
            }
            else
            {
                _logger.LogWarning("Request timeout (120s) for {Title}", title);
            }
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while searching for {Title}", title);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while searching for {Title}", title);
            return null;
        }
        finally
        {
            _requestSemaphore.Release();
        }
    }

    /// <summary>
    /// Получает фильм или сериал по ID из PoiskKino API.
    /// </summary>
    /// <param name="id">ID фильма/сериала.</param>
    /// <param name="apiKey">API-ключ для аутентификации.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Данные фильма или null, если не найдено или произошла ошибка.</returns>
    public async Task<PoiskKinoMovieDtoV1_4?> GetMovieByIdAsync(
        int id,
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("API key is not configured");
            return null;
        }

        var cacheKey = $"movie:{id}";
        
        // Сначала проверяем кэш
        if (_cache.TryGetValue(cacheKey, out var cachedEntry))
        {
            if (cachedEntry.ExpiresAt > DateTime.UtcNow)
            {
                _logger.LogDebug("Cache hit for movie ID {Id}", id);
                return cachedEntry.Data as PoiskKinoMovieDtoV1_4;
            }
            
            // Удаляем устаревшую запись
            _cache.TryRemove(cacheKey, out _);
        }

        // Выполняем API-запрос
        try
        {
            await _requestSemaphore.WaitAsync(cancellationToken);
            
            var url = $"/v1.4/movie/{id}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-API-KEY", apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                var errorMessage = await GetErrorMessageAsync(response, cancellationToken);
                _logger.LogWarning("API limit exceeded ({StatusCode}) for movie ID {Id}: {Message}", (int)response.StatusCode, id, errorMessage);
                return null;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogDebug("Not found (404) for movie ID {Id}", id);
                // Кэшируем отрицательный результат на меньшее время
                _cache.TryAdd(cacheKey, new CacheEntry<object>(null, DateTime.UtcNow.AddHours(1)));
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("API request failed with status {StatusCode} for movie ID {Id}", response.StatusCode, id);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var movieData = JsonSerializer.Deserialize<PoiskKinoMovieDtoV1_4>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Кэшируем успешный результат
            if (movieData != null)
            {
                _cache.TryAdd(cacheKey, new CacheEntry<object>(movieData, DateTime.UtcNow.AddHours(CacheExpirationHours)));
            }

            return movieData;
        }
        catch (TaskCanceledException)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Request cancelled by user for movie ID {Id}", id);
            }
            else
            {
                _logger.LogWarning("Request timeout (120s) for movie ID {Id}", id);
            }
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while getting movie ID {Id}", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting movie ID {Id}", id);
            return null;
        }
        finally
        {
            _requestSemaphore.Release();
        }
    }

    /// <summary>
    /// Получает данные сезона с эпизодами из PoiskKino API.
    /// </summary>
    /// <param name="movieId">ID фильма/сериала.</param>
    /// <param name="seasonNumber">Номер сезона.</param>
    /// <param name="apiKey">API-ключ для аутентификации.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Данные сезона с эпизодами или null, если не найдено или произошла ошибка.</returns>
    public async Task<PoiskKinoSeason?> GetSeasonAsync(
        int movieId,
        int seasonNumber,
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("API key is not configured");
            return null;
        }

        var cacheKey = $"season:{movieId}:{seasonNumber}";
        
        // Сначала проверяем кэш
        if (_cache.TryGetValue(cacheKey, out var cachedEntry))
        {
            if (cachedEntry.ExpiresAt > DateTime.UtcNow)
            {
                _logger.LogDebug("Cache hit for season {SeasonNumber} of movie ID {MovieId}", seasonNumber, movieId);
                return cachedEntry.Data as PoiskKinoSeason;
            }
            
            // Удаляем устаревшую запись
            _cache.TryRemove(cacheKey, out _);
        }

        // Выполняем API-запрос
        try
        {
            await _requestSemaphore.WaitAsync(cancellationToken);
            
            // Согласно документации, параметр называется "number", а не "seasonNumber"
            var url = $"/v1.4/season?movieId={movieId}&number={seasonNumber}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-API-KEY", apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                var errorMessage = await GetErrorMessageAsync(response, cancellationToken);
                _logger.LogWarning("API limit exceeded ({StatusCode}) for season {SeasonNumber} of movie ID {MovieId}: {Message}", (int)response.StatusCode, seasonNumber, movieId, errorMessage);
                return null;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogDebug("Not found (404) for season {SeasonNumber} of movie ID {MovieId}", seasonNumber, movieId);
                // Кэшируем отрицательный результат на меньшее время
                _cache.TryAdd(cacheKey, new CacheEntry<object>(null, DateTime.UtcNow.AddHours(1)));
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("API request failed with status {StatusCode} for season {SeasonNumber} of movie ID {MovieId}. URL: {Url}, Response: {ErrorBody}",
                    response.StatusCode, seasonNumber, movieId, url, errorBody);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var seasonResponse = JsonSerializer.Deserialize<PoiskKinoSeasonResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Извлекаем первый сезон из массива docs
            var seasonData = seasonResponse?.Docs?.FirstOrDefault();

            // Кэшируем успешный результат
            if (seasonData != null)
            {
                _cache.TryAdd(cacheKey, new CacheEntry<object>(seasonData, DateTime.UtcNow.AddHours(CacheExpirationHours)));
            }

            return seasonData;
        }
        catch (TaskCanceledException)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Request cancelled by user for season {SeasonNumber} of movie ID {MovieId}", seasonNumber, movieId);
            }
            else
            {
                _logger.LogWarning("Request timeout (120s) for season {SeasonNumber} of movie ID {MovieId}", seasonNumber, movieId);
            }
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while getting season {SeasonNumber} of movie ID {MovieId}", seasonNumber, movieId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting season {SeasonNumber} of movie ID {MovieId}", seasonNumber, movieId);
            return null;
        }
        finally
        {
            _requestSemaphore.Release();
        }
    }

    /// <summary>
    /// Генерирует ключ кэша из названия и года.
    /// </summary>
    private static string GenerateCacheKey(string title, int? year)
    {
        var normalizedTitle = title.Trim().ToLowerInvariant();
        return year.HasValue ? $"search:{normalizedTitle}:{year}" : $"search:{normalizedTitle}";
    }

    /// <summary>
    /// Извлекает сообщение об ошибке из ответа API.
    /// </summary>
    private static async Task<string> GetErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("message", out var messageElement))
            {
                return messageElement.GetString() ?? "Unknown error";
            }
        }
        catch
        {
            // Игнорируем ошибки парсинга
        }
        return "Unknown error";
    }

    /// <summary>
    /// Запись кэша с временем истечения.
    /// </summary>
    private class CacheEntry<T>(T? data, DateTime expiresAt)
    {
        public T? Data { get; } = data;
        public DateTime ExpiresAt { get; } = expiresAt;
    }
}

