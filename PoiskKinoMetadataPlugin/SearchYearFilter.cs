using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using PoiskKinoMetadataPlugin.Models;

[assembly: InternalsVisibleTo("PoiskKinoMetadataPlugin.UnitTests")]

namespace PoiskKinoMetadataPlugin;

/// <summary>
/// Локальная фильтрация результатов поиска по году на стороне плагина (ADR-0001).
/// </summary>
/// <remarks>
/// Год из «Распознать» Jellyfin должен детерминированно влиять на релевантность выдачи
/// независимо от поведения сервера: параметр <c>year</c> в <c>movie/search</c>
/// недокументирован ни в v1.4, ни в v1.5.
/// Фильтрация применяется в провайдерах, а не в <see cref="PoiskKinoApiClient"/>:
/// клиент остаётся транспортным адаптером без бизнес-правил, кэш хранит сырой ответ API.
/// Если после фильтрации пусто, а до неё были результаты — возвращается неотфильтрованный
/// список с debug-log (защита полноты при расхождениях годов источников метаданных).
/// </remarks>
internal static class SearchYearFilter
{
    /// <summary>
    /// Фильм: точное совпадение года выпуска.
    /// </summary>
    public static bool MatchesMovie(PoiskKinoItem item, int year) => item.Year == year;

    /// <summary>
    /// Сериал: год совпадает с годом начала или попадает в один из диапазонов <c>releaseYears</c>.
    /// </summary>
    public static bool MatchesSeries(PoiskKinoItem item, int year)
    {
        if (item.Year == year)
        {
            return true;
        }

        return item.ReleaseYears?.Any(range => CoversYear(range, year)) == true;
    }

    /// <summary>
    /// Применяет фильтр по году к результатам поиска с fallback на неотфильтрованный список.
    /// </summary>
    /// <param name="items">Результаты поиска после фильтрации по типу контента.</param>
    /// <param name="year">Год из поискового запроса; <c>null</c> — фильтрация не применяется.</param>
    /// <param name="predicate">Предикат совпадения года для конкретного типа контента.</param>
    /// <param name="logger">Логгер провайдера.</param>
    /// <returns>Отфильтрованный список или исходный, если фильтр отсёк всё.</returns>
    public static List<PoiskKinoItem> Apply(
        List<PoiskKinoItem> items,
        int? year,
        Func<PoiskKinoItem, int, bool> predicate,
        ILogger logger)
    {
        if (!year.HasValue || items.Count == 0)
        {
            return items;
        }

        var filtered = items.Where(item => predicate(item, year.Value)).ToList();
        if (filtered.Count > 0)
        {
            return filtered;
        }

        logger.LogDebug(
            "No search results matched year {Year}; returning unfiltered results ({Count} items)",
            year.Value,
            items.Count);
        return items;
    }

    private static bool CoversYear(PoiskKinoYearRange range, int year) =>
        (!range.Start.HasValue || range.Start.Value <= year) &&
        (!range.End.HasValue || range.End.Value >= year);
}
