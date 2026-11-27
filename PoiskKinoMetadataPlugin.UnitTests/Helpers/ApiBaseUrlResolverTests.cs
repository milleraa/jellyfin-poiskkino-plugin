namespace PoiskKinoMetadataPlugin.UnitTests.Helpers;

/// <summary>
/// Тесты политики нормализации/валидации базового URL API (D-ARCH-1, ADR-0003, задача 001).
/// </summary>
/// <remarks>
/// Покрывают правила 1–10 из <c>docs/backlog/features/eu-api-endpoint/tasks/001-api-base-url-resolver.md</c>.
/// Резолвер — чистая функция без Jellyfin/HTTP/логгера, поэтому моки не нужны.
/// </remarks>
public class ApiBaseUrlResolverTests
{
    private static readonly Uri DefaultUri = new(ApiBaseUrlResolver.DefaultBaseUrl, UriKind.Absolute);

    /// <summary>
    /// Проверяет общие инварианты валидного результата: https, корневой путь, канонический вид строки.
    /// </summary>
    private static void AssertValid(ResolvedBaseUrl result, string expectedCanonical)
    {
        Assert.False(result.UsedFallback);
        Assert.Null(result.FallbackReason);
        Assert.Equal(Uri.UriSchemeHttps, result.BaseAddress.Scheme);
        Assert.Equal("/", result.BaseAddress.AbsolutePath);
        Assert.False(string.IsNullOrEmpty(result.BaseAddress.Host));
        Assert.Equal(expectedCanonical, result.BaseAddress.OriginalString);
        Assert.False(result.BaseAddress.OriginalString.EndsWith('/'));
        Assert.Equal(new Uri(expectedCanonical, UriKind.Absolute), result.BaseAddress);
    }

    /// <summary>
    /// Проверяет общие инварианты fallback-результата: дефолтный адрес и непустая причина.
    /// </summary>
    private static void AssertFallback(ResolvedBaseUrl result)
    {
        Assert.True(result.UsedFallback);
        Assert.False(string.IsNullOrWhiteSpace(result.FallbackReason));
        Assert.Equal(DefaultUri, result.BaseAddress);
        Assert.Equal(ApiBaseUrlResolver.DefaultBaseUrl, result.BaseAddress.OriginalString);
    }

    // Правило 1 / AC-1: дефолт объявлен один раз и равен https://api.poiskkino.dev.
    [Fact]
    public void DefaultBaseUrl_IsPoiskkinoDev()
    {
        Assert.Equal("https://api.poiskkino.dev", ApiBaseUrlResolver.DefaultBaseUrl);

        var result = ApiBaseUrlResolver.Resolve(null);

        Assert.Equal(ApiBaseUrlResolver.DefaultBaseUrl, result.BaseAddress.OriginalString);
        Assert.Equal("api.poiskkino.dev", result.BaseAddress.Host);
        Assert.Equal(443, result.BaseAddress.Port);
    }

    // Правило 1 / AC-5: незаданное значение — не ошибка конфигурации, fallback не используется.
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    [InlineData(" \t\r\n ")]
    public void Resolve_MissingValue_ReturnsDefaultWithoutFallback(string? rawValue)
    {
        var result = ApiBaseUrlResolver.Resolve(rawValue);

        Assert.False(result.UsedFallback);
        Assert.Null(result.FallbackReason);
        Assert.Equal(DefaultUri, result.BaseAddress);
        Assert.Equal(ApiBaseUrlResolver.DefaultBaseUrl, result.BaseAddress.OriginalString);
    }

    // Правило 2: пробелы по краям обрезаются (схема и без схемы).
    [Theory]
    [InlineData("  https://eu-api.poiskkino.dev  ", "https://eu-api.poiskkino.dev")]
    [InlineData("\tapi.poiskkino.dev\n", "https://api.poiskkino.dev")]
    [InlineData(" eu-api.poiskkino.dev:8443/ ", "https://eu-api.poiskkino.dev:8443")]
    public void Resolve_SurroundingWhitespace_Trimmed(string rawValue, string expectedCanonical)
    {
        var result = ApiBaseUrlResolver.Resolve(rawValue);

        AssertValid(result, expectedCanonical);
    }

    // Правило 3: отсутствие схемы — подставляется https://.
    [Theory]
    [InlineData("api.poiskkino.dev", "https://api.poiskkino.dev")]
    [InlineData("eu-api.poiskkino.dev", "https://eu-api.poiskkino.dev")]
    [InlineData("EU-API.POISKKINO.DEV", "https://eu-api.poiskkino.dev")]
    [InlineData("api.poiskkino.dev:8443", "https://api.poiskkino.dev:8443")]
    [InlineData("localhost", "https://localhost")]
    [InlineData("127.0.0.1", "https://127.0.0.1")]
    public void Resolve_MissingScheme_PrependsHttps(string rawValue, string expectedCanonical)
    {
        var result = ApiBaseUrlResolver.Resolve(rawValue);

        AssertValid(result, expectedCanonical);
    }

    // Правило 4 / AC-4: любая схема, кроме https, — дефолт с причиной, без исключений.
    [Theory]
    [InlineData("http://api.poiskkino.dev")]
    [InlineData("http://eu-api.poiskkino.dev:8443")]
    [InlineData("HTTP://API.POISKKINO.DEV")]
    [InlineData("ftp://api.poiskkino.dev")]
    [InlineData("ws://api.poiskkino.dev")]
    [InlineData("file://api.poiskkino.dev")]
    [InlineData("file:///etc/passwd")]
    [InlineData("mailto:user@example.com")]
    [InlineData("javascript:alert(1)")]
    public void Resolve_NonHttpsScheme_FallsBackToDefault(string rawValue)
    {
        var result = ApiBaseUrlResolver.Resolve(rawValue);

        AssertFallback(result);
    }

    // Правило 5 / AC-4: пустой host или неразбираемая строка — дефолт с причиной.
    [Theory]
    [InlineData("not a url")]
    [InlineData("https://")]
    [InlineData("https:///v1.5")]
    [InlineData("https://api .poiskkino.dev")]
    [InlineData("https://:8443")]
    [InlineData("https://api.poiskkino.dev:99999")]
    [InlineData("://")]
    [InlineData("://host")]
    [InlineData("{}")]
    [InlineData("//api.poiskkino.dev")]
    [InlineData("\\\\server\\share")]
    public void Resolve_UnparsableOrEmptyHost_FallsBackToDefault(string rawValue)
    {
        var result = ApiBaseUrlResolver.Resolve(rawValue);

        AssertFallback(result);
    }

    // Правило 6 / AC-4: userinfo, query и fragment не имеют смысла в base address.
    [Theory]
    [InlineData("https://u:p@api.poiskkino.dev")]
    [InlineData("https://user@api.poiskkino.dev")]
    [InlineData("https://u:p@api.poiskkino.dev:8443")]
    [InlineData("https://api.poiskkino.dev?x=1")]
    [InlineData("https://api.poiskkino.dev/?x=1")]
    [InlineData("https://api.poiskkino.dev?")]
    [InlineData("https://api.poiskkino.dev#frag")]
    [InlineData("https://api.poiskkino.dev/#frag")]
    public void Resolve_UserInfoQueryOrFragment_FallsBackToDefault(string rawValue)
    {
        var result = ApiBaseUrlResolver.Resolve(rawValue);

        AssertFallback(result);
    }

    // Правило 7 / AC-4: непустой путь отвергается (D-ARCH-3), в том числе подпуть прокси.
    [Theory]
    [InlineData("https://api.poiskkino.dev/v1.5")]
    [InlineData("https://api.poiskkino.dev/v1.5/")]
    [InlineData("https://proxy.corp/poiskkino")]
    [InlineData("https://proxy.corp/poiskkino/")]
    [InlineData("https://api.poiskkino.dev/movie/search")]
    [InlineData("api.poiskkino.dev/prefix")]
    [InlineData("https://api.poiskkino.dev//")]
    public void Resolve_NonRootPath_FallsBackToDefault(string rawValue)
    {
        var result = ApiBaseUrlResolver.Resolve(rawValue);

        AssertFallback(result);
    }

    // Правило 8 / AC-3: завершающие "/" удаляются, результат — https://host[:port].
    [Theory]
    [InlineData("https://eu-api.poiskkino.dev/", "https://eu-api.poiskkino.dev")]
    [InlineData("https://eu-api.poiskkino.dev:8443/", "https://eu-api.poiskkino.dev:8443")]
    [InlineData("eu-api.poiskkino.dev/", "https://eu-api.poiskkino.dev")]
    [InlineData("api.poiskkino.dev/", "https://api.poiskkino.dev")]
    [InlineData("https://api.poiskkino.dev", "https://api.poiskkino.dev")]
    public void Resolve_TrailingSlash_Removed(string rawValue, string expectedCanonical)
    {
        var result = ApiBaseUrlResolver.Resolve(rawValue);

        AssertValid(result, expectedCanonical);
    }

    // Правило 9 / AC-4: явный порт сохраняется, дефолтный не дублируется в канонической строке.
    [Theory]
    [InlineData("https://eu-api.poiskkino.dev:8443", 8443, "https://eu-api.poiskkino.dev:8443")]
    [InlineData("eu-api.poiskkino.dev:8443", 8443, "https://eu-api.poiskkino.dev:8443")]
    [InlineData("https://api.poiskkino.dev:8080/", 8080, "https://api.poiskkino.dev:8080")]
    [InlineData("https://api.poiskkino.dev:80", 80, "https://api.poiskkino.dev:80")]
    [InlineData("https://api.poiskkino.dev:443", 443, "https://api.poiskkino.dev")]
    [InlineData("https://api.poiskkino.dev", 443, "https://api.poiskkino.dev")]
    public void Resolve_ExplicitPort_Preserved(string rawValue, int expectedPort, string expectedCanonical)
    {
        var result = ApiBaseUrlResolver.Resolve(rawValue);

        AssertValid(result, expectedCanonical);
        Assert.Equal(expectedPort, result.BaseAddress.Port);
    }

    // Правило 10: регистр нормализует Uri — host в нижнем регистре, схема распознаётся без учёта регистра.
    [Theory]
    [InlineData("https://EU-API.POISKKINO.DEV", "eu-api.poiskkino.dev")]
    [InlineData("HTTPS://Api.PoiskKino.Dev", "api.poiskkino.dev")]
    [InlineData("https://API.POISKKINO.DEV/", "api.poiskkino.dev")]
    public void Resolve_HostCase_NormalizedToLowerCase(string rawValue, string expectedHost)
    {
        var result = ApiBaseUrlResolver.Resolve(rawValue);

        Assert.False(result.UsedFallback);
        Assert.Equal(expectedHost, result.BaseAddress.Host);
        Assert.Equal(Uri.UriSchemeHttps, result.BaseAddress.Scheme);
    }

    // Правило 10: IDN-хост нормализуется в punycode самим Uri, каноническая строка — ASCII.
    [Fact]
    public void Resolve_IdnHost_NormalizedToPunycode()
    {
        var expectedPunycode = new Uri("https://мвд.рф", UriKind.Absolute).IdnHost;

        var result = ApiBaseUrlResolver.Resolve("https://мвд.рф");

        Assert.False(result.UsedFallback);
        Assert.Null(result.FallbackReason);
        Assert.Equal(Uri.UriSchemeHttps, result.BaseAddress.Scheme);
        Assert.Equal(expectedPunycode, result.BaseAddress.IdnHost);
        Assert.StartsWith("xn--", result.BaseAddress.IdnHost, StringComparison.Ordinal);
        Assert.All(result.BaseAddress.OriginalString, c => Assert.True(c < 128, $"не-ASCII символ '{c}' в каноническом адресе"));
    }

    // AC-4: «Resolve не бросает исключение ни на одном входе» — контрпримеры, включая очень длинные строки.
    [Fact]
    public void Resolve_NeverThrows_ForHostileInputs()
    {
        var longTail = new string('a', 5000);
        var inputs = new[]
        {
            null,
            string.Empty,
            " ",
            longTail,
            "https://" + longTail + ".dev",
            "https://api.poiskkino.dev/" + longTail,
            "https://api.poiskkino.dev:" + longTail,
            "https://api.poiskkino.dev:-1",
            "https://api.poiskkino.dev:",
            "https://[::1]",
            "https://[::1]:8443",
            "https://[::1]/",
            "https://api.poiskkino.dev\t/",
            "https://api.poiskkino.dev /",
            "https://api.poiskkino.dev/../..",
            "https://api.poiskkino.dev/%2e%2e/",
            "https://api.poiskkino.dev:8443/v1.5?x=1#f",
            "https://user:pass word@api.poiskkino.dev",
            "https://😀.dev",
            "http://",
            " https://eu-api.poiskkino.dev ",
            "https://",
            ".",
            "..",
            "/",
            "///",
            "https://api.poiskkino.dev\n",
            "\u0000",
            "\u0007https://api.poiskkino.dev",
        };

        foreach (var rawValue in inputs)
        {
            var result = default(ResolvedBaseUrl);

            var exception = Record.Exception(() => result = ApiBaseUrlResolver.Resolve(rawValue));

            Assert.True(exception is null, $"Resolve бросил {exception?.GetType().Name} на входе '{Describe(rawValue)}'");
            Assert.NotNull(result.BaseAddress);
            Assert.Equal(Uri.UriSchemeHttps, result.BaseAddress.Scheme);
            Assert.True(result.BaseAddress.IsAbsoluteUri);
            Assert.False(string.IsNullOrEmpty(result.BaseAddress.Host));
            Assert.Equal("/", result.BaseAddress.AbsolutePath);
            Assert.Equal(result.UsedFallback, !string.IsNullOrEmpty(result.FallbackReason));
            Assert.True(
                result.UsedFallback || !result.BaseAddress.OriginalString.EndsWith('/'),
                $"валидный результат не должен содержать завершающий '/': '{result.BaseAddress.OriginalString}'");
        }
    }

    // AC-5: для любого валидного значения fallback не выставляется; причина есть только при fallback.
    [Theory]
    [InlineData("https://eu-api.poiskkino.dev")]
    [InlineData("api.poiskkino.dev")]
    [InlineData("https://eu-api.poiskkino.dev:8443/")]
    public void Resolve_ValidValue_DoesNotReportFallback(string rawValue)
    {
        var result = ApiBaseUrlResolver.Resolve(rawValue);

        Assert.False(result.UsedFallback);
        Assert.Null(result.FallbackReason);
        Assert.Equal(Uri.UriSchemeHttps, result.BaseAddress.Scheme);
    }

    private static string Describe(string? value) => value?.ReplaceLineEndings("\\n") ?? "<null>";
}
