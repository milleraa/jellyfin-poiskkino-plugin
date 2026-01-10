using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace PoiskKinoMetadataPlugin;

/// <summary>
/// Провайдер внешних идентификаторов для плагина PoiskKino.
/// Позволяет Jellyfin связывать медиа-элементы с идентификаторами PoiskKino.
/// </summary>
public class PoiskKinoExternalId : IExternalId
{
    /// <inheritdoc />
    public bool Supports(IHasProviderIds item)
        => item is Series || item is Movie;

    /// <inheritdoc />
    public string ProviderName
        => "ПоискКино";

    /// <inheritdoc />
    public string Key
        => ProviderNames.PoiskKino;

    /// <inheritdoc />
    public ExternalIdMediaType? Type
        => ExternalIdMediaType.Series;

    /// <inheritdoc />
    public string? UrlFormatString
        => null;
}

