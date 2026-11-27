using MediaBrowser.Model.Plugins;

namespace PoiskKinoMetadataPlugin;

/// <summary>
/// Конфигурация плагина PoiskKino.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Получает или задаёт API-ключ PoiskKino.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Получает или задаёт базовый URL API PoiskKino; для серверов вне РФ можно указать европейское зеркало API.
    /// </summary>
    /// <remarks>
    /// Хранится ровно в том виде, как ввёл администратор; пустое значение и значение по умолчанию означают
    /// <see cref="ApiBaseUrlResolver.DefaultBaseUrl"/>. Нормализация и валидация выполняются в
    /// <see cref="ApiBaseUrlResolver"/> при создании <see cref="PoiskKinoApiClient"/>, обратно в конфигурацию
    /// значение не записывается (ADR-0003, D-ARCH-4). Имя свойства — часть XML-схемы конфигурации.
    /// </remarks>
    public string ApiBaseUrl { get; set; } = ApiBaseUrlResolver.DefaultBaseUrl;

    /// <summary>
    /// Получает или задаёт значение, указывающее, нужно ли игнорировать изображения с домена tmdb.org.
    /// Полезно для регионов, где TMDB заблокирован или имеет проблемы с доступом.
    /// </summary>
    public bool IgnoreTmdbImages { get; set; } = true;
}

