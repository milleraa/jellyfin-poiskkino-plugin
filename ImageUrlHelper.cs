namespace PoiskKinoMetadataPlugin;

/// <summary>
/// Вспомогательный класс для фильтрации и валидации URL изображений.
/// </summary>
public static class ImageUrlHelper
{
    /// <summary>
    /// Проверяет, принадлежит ли URL домену tmdb.org.
    /// </summary>
    /// <param name="url">URL для проверки.</param>
    /// <returns>True, если URL содержит tmdb.org, иначе false.</returns>
    public static bool IsTmdbUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        return url.Contains("tmdb.org", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Проверяет, нужно ли игнорировать изображения TMDB на основе конфигурации плагина.
    /// </summary>
    /// <returns>True, если изображения TMDB нужно игнорировать, иначе false.</returns>
    public static bool ShouldIgnoreTmdbImages()
    {
        return Plugin.Instance?.Configuration?.IgnoreTmdbImages == true;
    }

    /// <summary>
    /// Проверяет, нужно ли отфильтровать URL на основе текущей конфигурации.
    /// </summary>
    /// <param name="url">URL для проверки.</param>
    /// <returns>True, если URL нужно отфильтровать (игнорировать), иначе false.</returns>
    public static bool ShouldFilterUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        return ShouldIgnoreTmdbImages() && IsTmdbUrl(url);
    }
}
