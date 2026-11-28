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
    private const int CacheExpirationHours = 24;
    private const int MaxRequestsPerDay = 200;
    private const int MaxLoggedRawBaseUrlLength = 200;

    private readonly HttpClient _httpClient;
    private readonly ILogger<PoiskKinoApiClient> _logger;
    private readonly ConcurrentDictionary<string, CacheEntry<object>> _cache;
    private readonly SemaphoreSlim _requestSemaphore;

    // Памятка дедупликации Warning о невалидном лимите (решение A1, ADR-0004):
    // «последнее предупреждённое невалидное значение»; int.MinValue означает «предупреждений ещё не было».
    private int _lastWarnedInvalidSearchLimit = int.MinValue;

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="PoiskKinoApiClient"/>.
    /// </summary>
    /// <param name="httpClientFactory">Фабрика HTTP-клиентов.</param>
    /// <param name="logger">Экземпляр логгера.</param>
    /// <param name="apiBaseUrl">
    /// Сырое значение базового URL из конфигурации плагина «как ввёл администратор»;
    /// <c>null</c> или пустое значение — <see cref="ApiBaseUrlResolver.DefaultBaseUrl"/>.
    /// Нормализация и валидация выполняются один раз, при создании клиента, в <see cref="ApiBaseUrlResolver"/>.
    /// </param>
    public PoiskKinoApiClient(
        IHttpClientFactory httpClientFactory,
        ILogger<PoiskKinoApiClient> logger,
        string? apiBaseUrl = null)
    {
        _logger = logger;

        // Единственная точка резолва: исключений нет, невалидное значение даёт дефолт + причину (D-ARCH-1).
        var resolvedBaseUrl = ApiBaseUrlResolver.Resolve(apiBaseUrl);

        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = resolvedBaseUrl.BaseAddress;
        _httpClient.Timeout = TimeSpan.FromSeconds(120);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Jellyfin-PoiskKino-Plugin/1.0");

        _logger.LogInformation("PoiskKino API base URL: {BaseUrl}", _httpClient.BaseAddress);

        if (resolvedBaseUrl.UsedFallback)
        {
            _logger.LogWarning(
                "Invalid API base URL '{RawApiBaseUrl}' (truncated to {MaxLength} chars), falling back to '{DefaultBaseUrl}': {Reason}",
                TruncateForLog(RedactUserInfo(apiBaseUrl), MaxLoggedRawBaseUrlLength),
                MaxLoggedRawBaseUrlLength,
                ApiBaseUrlResolver.DefaultBaseUrl,
                resolvedBaseUrl.FallbackReason);
        }

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
    /// <param name="searchLimit">
    /// Сырое значение лимита результатов поиска из конфигурации. Валидируется в <see cref="SearchLimitResolver"/>:
    /// значение 1–10 применяется как есть; значение вне диапазона — <see cref="SearchLimitResolver.DefaultLimit"/>
    /// с <c>Warning</c>, дедуплицируемым по невалидному значению. Один и тот же эффективный лимит идёт
    /// и в URL (<c>limit=N</c>), и в ключ кэша (<c>:N</c>).
    /// </param>
    /// <returns>Ответ с результатами поиска или null, если не найдено или произошла ошибка.</returns>
    public async Task<PoiskKinoSearchResponse?> SearchAsync(
        string title,
        int? year,
        string apiKey,
        CancellationToken cancellationToken = default,
        int searchLimit = SearchLimitResolver.DefaultLimit)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("API key is not configured");
            return null;
        }

        // Единственная точка резолва лимита: исключений нет, невалидное значение даёт дефолт + причину (ADR-0004).
        var resolvedLimit = SearchLimitResolver.Resolve(searchLimit);

        if (resolvedLimit.UsedFallback)
        {
            WarnInvalidSearchLimit(searchLimit, resolvedLimit.FallbackReason);
        }
        else
        {
            // Валидное значение сбрасывает памятку: эпизод «исправил и снова сломал» даёт новое предупреждение (A1).
            _lastWarnedInvalidSearchLimit = int.MinValue;
        }

        var cacheKey = GenerateCacheKey(title, year, resolvedLimit.EffectiveLimit);
        
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
            
            var url = $"/v1.5/movie/search?query={Uri.EscapeDataString(title)}&limit={resolvedLimit.EffectiveLimit}";
            if (year.HasValue)
            {
                url += $"&year={year.Value}";
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-API-KEY", apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var errorMessage = await GetErrorMessageAsync(response, cancellationToken);
                _logger.LogError("API key invalid or missing for {Title}: {Message}", title, errorMessage);
                return null;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                var errorMessage = await GetErrorMessageAsync(response, cancellationToken);
                _logger.LogWarning("API rate limit exceeded ({StatusCode}) for {Title}: {Message}", (int)response.StatusCode, title, errorMessage);
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
    public async Task<PoiskKinoMovieDto?> GetMovieByIdAsync(
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
                return cachedEntry.Data as PoiskKinoMovieDto;
            }
            
            // Удаляем устаревшую запись
            _cache.TryRemove(cacheKey, out _);
        }

        // Выполняем API-запрос
        try
        {
            await _requestSemaphore.WaitAsync(cancellationToken);
            
            var url = $"/v1.5/movie/{id}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-API-KEY", apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var errorMessage = await GetErrorMessageAsync(response, cancellationToken);
                _logger.LogError("API key invalid or missing for movie ID {Id}: {Message}", id, errorMessage);
                return null;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                var errorMessage = await GetErrorMessageAsync(response, cancellationToken);
                _logger.LogWarning("API rate limit exceeded ({StatusCode}) for movie ID {Id}: {Message}", (int)response.StatusCode, id, errorMessage);
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
            var movieData = JsonSerializer.Deserialize<PoiskKinoMovieDto>(content, new JsonSerializerOptions
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
            
            var url = $"/v1.5/season?movieId={movieId}&number={seasonNumber}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-API-KEY", apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var errorMessage = await GetErrorMessageAsync(response, cancellationToken);
                _logger.LogError("API key invalid or missing for season {SeasonNumber} of movie ID {MovieId}: {Message}", seasonNumber, movieId, errorMessage);
                return null;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                var errorMessage = await GetErrorMessageAsync(response, cancellationToken);
                _logger.LogWarning("API rate limit exceeded ({StatusCode}) for season {SeasonNumber} of movie ID {MovieId}: {Message}", (int)response.StatusCode, seasonNumber, movieId, errorMessage);
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
            var seasonResponse = JsonSerializer.Deserialize<PoiskKinoSeasonCursorResponse>(content, new JsonSerializerOptions
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
    /// Удаляет <c>userinfo</c> (логин и пароль) из сырого значения перед выводом в лог.
    /// </summary>
    /// <param name="rawValue">Сырое значение из конфигурации «как ввёл администратор».</param>
    /// <returns>Значение без <c>userinfo</c> в authority (с сохранением схемы) либо без изменений.</returns>
    /// <remarks>
    /// Резолвер отклоняет адреса с <c>userinfo</c>, но в <c>Warning</c> попадает сырая строка,
    /// поэтому пароль из <c>https://user:password@host</c> мог бы оказаться в логе (риск A6, ADR-0003).
    /// Обрабатывается и значение без схемы (<c>user:password@host</c>).
    /// <c>userinfo</c> заканчивается последним <c>@</c> внутри authority (пароль сам может содержать <c>@</c>),
    /// <c>@</c> в пути, query или fragment не вырезается — это не credentials.
    /// </remarks>
    private static string RedactUserInfo(string? rawValue)
    {
        var value = rawValue ?? string.Empty;

        var schemeSeparatorIndex = value.IndexOf("://", StringComparison.Ordinal);
        var authorityStart = schemeSeparatorIndex < 0 ? 0 : schemeSeparatorIndex + 3;

        // Authority кончается на первом '/', '?' или '#' (или на конце строки).
        var authorityEndIndex = value.IndexOfAny(['/', '?', '#'], authorityStart);
        var authorityEnd = authorityEndIndex < 0 ? value.Length : authorityEndIndex;
        if (authorityEnd <= authorityStart)
        {
            // Пустой authority ("https://", "https:///x") — вырезать нечего, но и падать нельзя.
            return value;
        }

        // Берём ПОСЛЕДНИЙ '@' в authority: пароль может содержать '@' (admin:p@ss@host).
        var atIndex = value.LastIndexOf('@', authorityEnd - 1, authorityEnd - authorityStart);
        if (atIndex < 0)
        {
            return value;
        }

        var schemePrefix = schemeSeparatorIndex < 0 ? string.Empty : value[..authorityStart];
        return string.Concat(schemePrefix, value.AsSpan(atIndex + 1));
    }

    /// <summary>
    /// Обрезает сырое значение из конфигурации для безопасного вывода в лог.
    /// </summary>
    /// <param name="rawValue">Сырое значение из конфигурации.</param>
    /// <param name="maxLength">Максимальная длина результата в символах.</param>
    /// <returns>Значение без изменений или его первые <paramref name="maxLength"/> символов.</returns>
    private static string TruncateForLog(string? rawValue, int maxLength)
    {
        var value = rawValue ?? string.Empty;
        return value.Length <= maxLength ? value : value[..maxLength];
    }

    /// <summary>
    /// Генерирует ключ кэша из названия, года и эффективного лимита поиска.
    /// </summary>
    /// <remarks>
    /// Лимит присутствует в ключе всегда (без условного опускания при дефолте — ADR-0004, вариант А):
    /// пара «запрос <c>limit=N</c> ⇔ ключ <c>:N</c>» детерминирована, записи с разными лимитами не смешиваются.
    /// </remarks>
    private static string GenerateCacheKey(string title, int? year, int searchLimit)
    {
        var normalizedTitle = title.Trim().ToLowerInvariant();
        return year.HasValue
            ? $"search:{normalizedTitle}:{year}:{searchLimit}"
            : $"search:{normalizedTitle}:{searchLimit}";
    }

    /// <summary>
    /// Пишет <c>Warning</c> о невалидном лимите с дедупликацией по значению (решение A1, ADR-0004).
    /// </summary>
    /// <param name="rawSearchLimit">Сырое невалидное значение из конфигурации.</param>
    /// <param name="reason">Причина отказа из <see cref="SearchLimitResolver"/>.</param>
    /// <remarks>
    /// Памятка «последнее предупреждённое невалидное значение» живёт на экземпляре клиента — резолвер остаётся
    /// без состояния. Предупреждение пишется только при смене невалидного значения; валидное значение сбрасывает
    /// памятку (<see cref="SearchAsync"/>). Чтение/запись <c>int</c> атомарны; гонка в худшем случае даёт дубль
    /// в логе — допустимо. API-ключ в сообщение не попадает.
    /// </remarks>
    private void WarnInvalidSearchLimit(int rawSearchLimit, string? reason)
    {
        if (_lastWarnedInvalidSearchLimit == rawSearchLimit)
        {
            return;
        }

        _lastWarnedInvalidSearchLimit = rawSearchLimit;
        _logger.LogWarning(
            "Invalid search limit {RawSearchLimit} ({Reason}), falling back to default {DefaultSearchLimit}",
            rawSearchLimit,
            reason,
            SearchLimitResolver.DefaultLimit);
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

