namespace PoiskKinoMetadataPlugin;

/// <summary>
/// Результат валидации лимита результатов поиска.
/// </summary>
/// <param name="EffectiveLimit">
/// Эффективный лимит: сырое значение из диапазона 1–10 как есть либо
/// <see cref="SearchLimitResolver.DefaultLimit"/> при fallback.
/// Одно и то же значение идёт и в URL (<c>limit=N</c>), и в ключ кэша поиска (<c>:N</c>).
/// </param>
/// <param name="UsedFallback">
/// <c>true</c>, если исходное значение вне диапазона и применён <see cref="SearchLimitResolver.DefaultLimit"/>.
/// </param>
/// <param name="FallbackReason">
/// Короткая причина отказа для лога (<c>Warning</c>) или <c>null</c>, если отказа не было.
/// </param>
internal readonly record struct ResolvedSearchLimit(int EffectiveLimit, bool UsedFallback, string? FallbackReason);

/// <summary>
/// Единственная точка политики валидации лимита результатов поиска (ADR-0004).
/// </summary>
/// <remarks>
/// <para>
/// Правила (детерминированные, без исключений наружу):
/// </para>
/// <list type="number">
/// <item><description>значение в диапазоне [<see cref="MinLimit"/>, <see cref="MaxLimit"/>] применяется как есть;</description></item>
/// <item><description>значение вне диапазона (0, отрицательные, больше <see cref="MaxLimit"/>) —
/// дефолт <see cref="DefaultLimit"/> + причина отказа (fallback, не clamping — отвергнутая альтернатива ADR-0004).</description></item>
/// </list>
/// <para>
/// Класс — чистая функция без состояния (по образцу <see cref="ApiBaseUrlResolver"/>): не зависит от Jellyfin,
/// <c>HttpClient</c>, логгера и <c>Plugin.Instance</c> и ничего не сохраняет. Ограничения UI (min/max) — только UX,
/// авторитетная валидация — здесь. Дедупликация <c>Warning</c> выполняется в <see cref="PoiskKinoApiClient"/>,
/// не в резолвере.
/// </para>
/// </remarks>
internal static class SearchLimitResolver
{
    /// <summary>
    /// Минимально допустимый лимит результатов поиска.
    /// </summary>
    public const int MinLimit = 1;

    /// <summary>
    /// Максимально допустимый лимит результатов поиска.
    /// </summary>
    public const int MaxLimit = 10;

    /// <summary>
    /// Лимит по умолчанию (сохраняет историческое поведение поиска).
    /// </summary>
    public const int DefaultLimit = 3;

    /// <summary>
    /// Валидирует сырое значение лимита результатов поиска из конфигурации.
    /// </summary>
    /// <param name="rawValue">Сырое значение «как сохранил администратор».</param>
    /// <returns>
    /// Всегда валидный результат: либо сырое значение из диапазона, либо <see cref="DefaultLimit"/>
    /// с <see cref="ResolvedSearchLimit.UsedFallback"/> и причиной отказа.
    /// </returns>
    public static ResolvedSearchLimit Resolve(int rawValue)
    {
        if (rawValue is >= MinLimit and <= MaxLimit)
        {
            return new ResolvedSearchLimit(rawValue, false, null);
        }

        var reason = rawValue < MinLimit
            ? $"value {rawValue} is below the allowed minimum {MinLimit}"
            : $"value {rawValue} is above the allowed maximum {MaxLimit}";
        return new ResolvedSearchLimit(DefaultLimit, true, reason);
    }
}
