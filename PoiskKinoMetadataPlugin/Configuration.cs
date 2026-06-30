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
    /// Получает или задаёт значение, указывающее, нужно ли игнорировать изображения с домена tmdb.org.
    /// Полезно для регионов, где TMDB заблокирован или имеет проблемы с доступом.
    /// </summary>
    public bool IgnoreTmdbImages { get; set; } = true;
}

