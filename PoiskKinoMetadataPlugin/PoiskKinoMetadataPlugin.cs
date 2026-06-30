using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;

namespace PoiskKinoMetadataPlugin;

/// <summary>
/// Главный класс плагина для провайдера метаданных PoiskKino.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    /// <summary>
    /// Получает экземпляр плагина.
    /// </summary>
    public static Plugin? Instance { get; private set; }

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="Plugin"/>.
    /// </summary>
    public Plugin(
        IApplicationPaths applicationPaths,
        IXmlSerializer xmlSerializer,
        ILogger<Plugin> logger)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
        logger.LogInformation("PoiskKino Metadata Plugin loaded");
    }

    /// <inheritdoc />
    public override string Name => "ПоискКино Metadata";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("A1B2C3D4-E5F6-7890-ABCD-EF1234567890");

    /// <inheritdoc />
    public override string Description => "Метаданные фильмов и сериалов на русском языке через API ПоискКино";

    /// <inheritdoc />
    public PluginConfiguration PluginConfiguration => Configuration;

    /// <inheritdoc />
    public Stream GetEmbeddedImageStream()
    {
        var imagePath = GetEmbeddedImagePath();
        if (File.Exists(imagePath))
        {
            return File.OpenRead(imagePath);
        }
        return Stream.Null;
    }

    /// <inheritdoc />
    public string GetEmbeddedImagePath()
    {
        return Path.Combine(GetPluginDirectory(), "icon.png");
    }

    /// <inheritdoc />
    public Stream GetThumbImage()
    {
        var thumbPath = GetThumbImagePath();
        if (File.Exists(thumbPath))
        {
            return File.OpenRead(thumbPath);
        }
        return Stream.Null;
    }

    /// <inheritdoc />
    public string GetThumbImagePath()
    {
        return Path.Combine(GetPluginDirectory(), "thumb.png");
    }

    private string GetPluginDirectory()
    {
        return Path.GetDirectoryName(GetType().Assembly.Location) ?? string.Empty;
    }

    /// <summary>
    /// Получает конфигурацию веб-страниц для настроек плагина.
    /// </summary>
    public IEnumerable<PluginPageInfo> GetPages()
    {
        return new[]
        {
            new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = GetType().Namespace + ".Configuration.configPage.html"
            }
        };
    }

}

// Примечание: В Jellyfin 10.9+ провайдеры метаданных автоматически обнаруживаются
// системой плагинов, когда они реализуют IRemoteMetadataProvider или IRemoteImageProvider.
// Система плагинов автоматически регистрирует и создаёт экземпляры этих провайдеров.
// PoiskKinoApiClient будет создан через внедрение зависимостей при создании провайдеров.

