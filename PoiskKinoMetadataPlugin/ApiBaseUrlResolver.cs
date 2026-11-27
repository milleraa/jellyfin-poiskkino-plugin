using System.Globalization;

namespace PoiskKinoMetadataPlugin;

/// <summary>
/// Результат нормализации базового URL API.
/// </summary>
/// <param name="BaseAddress">
/// Абсолютный <see cref="Uri"/> со схемой <c>https</c>, корневым путём (<c>/</c>), без query и fragment.
/// В <see cref="Uri.OriginalString"/> завершающий <c>/</c> отсутствует: <c>https://host[:port]</c>.
/// </param>
/// <param name="UsedFallback">
/// <c>true</c>, если исходное значение отклонено и применён <see cref="ApiBaseUrlResolver.DefaultBaseUrl"/>.
/// </param>
/// <param name="FallbackReason">
/// Короткая причина отказа для лога (<c>Warning</c>) или <c>null</c>, если отказа не было.
/// </param>
internal readonly record struct ResolvedBaseUrl(Uri BaseAddress, bool UsedFallback, string? FallbackReason);

/// <summary>
/// Единственная точка политики базового URL PoiskKino API (ADR-0003).
/// </summary>
/// <remarks>
/// <para>
/// Правила нормализации и валидации (детерминированные, без исключений наружу):
/// </para>
/// <list type="number">
/// <item><description><c>null</c>, пустая строка и строка из пробелов — дефолт без fallback (значение не задано, это не ошибка).</description></item>
/// <item><description>Пробелы по краям обрезаются.</description></item>
/// <item><description>Если в строке нет <c>://</c>, подставляется схема <c>https://</c>.</description></item>
/// <item><description>Любая схема, кроме <c>https</c> (<c>http</c>, <c>ftp</c>, <c>file</c>, …), — дефолт + причина.</description></item>
/// <item><description>Пустой host или строка, не разбираемая как абсолютный URI, — дефолт + причина.</description></item>
/// <item><description><c>userinfo</c>, <c>query</c>, <c>fragment</c> — дефолт + причина.</description></item>
/// <item><description>Непустой путь (кроме <c>/</c>) — дефолт + причина: базовый URL должен указывать на корень хоста.</description></item>
/// <item><description>Завершающие <c>/</c> удаляются: результат — <c>https://host[:port]</c>.</description></item>
/// <item><description>Явный порт сохраняется (осознанный выбор администратора).</description></item>
/// <item><description>Регистр хоста и IDN нормализует сам <see cref="Uri"/> (host в нижнем регистре, punycode).</description></item>
/// </list>
/// <para>
/// Класс не зависит от Jellyfin, <c>HttpClient</c>, логгера и <c>Plugin.Instance</c>, не знает про API-ключ
/// и ничего не сохраняет: нормализованное значение обратно в конфигурацию не записывается (D-ARCH-4).
/// </para>
/// </remarks>
internal static class ApiBaseUrlResolver
{
    /// <summary>
    /// Базовый URL API по умолчанию (единственное место хранения дефолта в коде).
    /// </summary>
    public const string DefaultBaseUrl = "https://api.poiskkino.dev";

    private const string SchemeSeparator = "://";

    private static readonly ResolvedBaseUrl DefaultResult =
        new(new Uri(DefaultBaseUrl, UriKind.Absolute), false, null);

    /// <summary>
    /// Нормализует и валидирует сырое значение базового URL из конфигурации.
    /// </summary>
    /// <param name="rawValue">Значение «как ввёл администратор»; может быть <c>null</c> или пустым.</param>
    /// <returns>
    /// Всегда валидный результат: либо нормализованный адрес, либо <see cref="DefaultBaseUrl"/>
    /// с <see cref="ResolvedBaseUrl.UsedFallback"/> и причиной отказа.
    /// </returns>
    public static ResolvedBaseUrl Resolve(string? rawValue)
    {
        var trimmed = rawValue?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return DefaultResult;
        }

        // Правило 3: схема определяется только по наличию "://" — IsWellFormedUriString
        // непредсказуемо отклоняет "host:port" и IDN-кейсы.
        var candidate = trimmed.Contains(SchemeSeparator, StringComparison.Ordinal)
            ? trimmed
            : Uri.UriSchemeHttps + SchemeSeparator + trimmed;

        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var parsed))
        {
            return Fallback("value cannot be parsed as an absolute URL");
        }

        if (!string.Equals(parsed.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            return Fallback($"scheme '{parsed.Scheme}' is not supported, only https is allowed");
        }

        if (string.IsNullOrEmpty(parsed.Host))
        {
            return Fallback("host is empty");
        }

        if (!string.IsNullOrEmpty(parsed.UserInfo))
        {
            return Fallback("userinfo is not supported in a base URL");
        }

        if (!string.IsNullOrEmpty(parsed.Query))
        {
            return Fallback("query string is not supported in a base URL");
        }

        if (!string.IsNullOrEmpty(parsed.Fragment))
        {
            return Fallback("fragment is not supported in a base URL");
        }

        if (parsed.AbsolutePath.Length > 1)
        {
            return Fallback($"path '{parsed.AbsolutePath}' is not supported, only the host root is allowed");
        }

        // Правила 8–10: корневой путь, без завершающего "/", порт только если он не дефолтный,
        // host уже в нижнем регистре и в punycode (Uri.IdnHost).
        var portSuffix = parsed.IsDefaultPort
            ? string.Empty
            : ":" + parsed.Port.ToString(CultureInfo.InvariantCulture);
        var canonical = $"{Uri.UriSchemeHttps}{SchemeSeparator}{parsed.IdnHost}{portSuffix}";

        if (!Uri.TryCreate(canonical, UriKind.Absolute, out var normalized))
        {
            return Fallback("value cannot be normalized into an absolute URL");
        }

        return new ResolvedBaseUrl(normalized, false, null);
    }

    private static ResolvedBaseUrl Fallback(string reason) =>
        new(DefaultResult.BaseAddress, true, reason);
}
