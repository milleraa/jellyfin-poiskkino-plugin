namespace PoiskKinoMetadataPlugin.UnitTests.Helpers;

/// <summary>
/// Тесты политики валидации лимита результатов поиска (ADR-0004, задача 002).
/// </summary>
/// <remarks>
/// Резолвер — чистая функция без Jellyfin/HTTP/логгера, поэтому моки не нужны.
/// Границы диапазона 1–10 и дефолт 3 — решение Стейхолдера (Q1, ADR-0004), не выводятся из реализации.
/// </remarks>
public class SearchLimitResolverTests
{
    // AC-1 (ADR-0004): константы политики объявлены в единственной точке.
    [Fact]
    public void PolicyConstants_AreStakeholderDecision()
    {
        Assert.Equal(1, SearchLimitResolver.MinLimit);
        Assert.Equal(10, SearchLimitResolver.MaxLimit);
        Assert.Equal(3, SearchLimitResolver.DefaultLimit);
    }

    // AC-4: границы 1 и 10 применяются как есть, без fallback.
    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    public void Resolve_BoundaryValues_AppliedAsIs(int rawValue)
    {
        var result = SearchLimitResolver.Resolve(rawValue);

        Assert.False(result.UsedFallback);
        Assert.Null(result.FallbackReason);
        Assert.Equal(rawValue, result.EffectiveLimit);
    }

    // AC-4: значения внутри диапазона применяются как есть (дефолт 3 валиден, произвольное 5 — тоже).
    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(7)]
    public void Resolve_InRangeValues_AppliedAsIs(int rawValue)
    {
        var result = SearchLimitResolver.Resolve(rawValue);

        Assert.False(result.UsedFallback);
        Assert.Null(result.FallbackReason);
        Assert.Equal(rawValue, result.EffectiveLimit);
    }

    // AC-4: 0, отрицательные и >10 — дефолт 3 + причина (fallback, не clamping: 11 не превращается в 10).
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(11)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void Resolve_OutOfRangeValue_FallsBackToDefaultWithReason(int rawValue)
    {
        var result = SearchLimitResolver.Resolve(rawValue);

        Assert.True(result.UsedFallback);
        Assert.False(string.IsNullOrWhiteSpace(result.FallbackReason));
        Assert.Equal(SearchLimitResolver.DefaultLimit, result.EffectiveLimit);
        Assert.Equal(3, result.EffectiveLimit);
    }

    // AC-4: clamping отвергнут ADR-0004 — 11 и 15 не «сжимаются» до 10, 0 не «поднимается» до 1.
    [Theory]
    [InlineData(11)]
    [InlineData(15)]
    [InlineData(0)]
    [InlineData(-5)]
    public void Resolve_OutOfRangeValue_DoesNotClampToNearestBoundary(int rawValue)
    {
        var result = SearchLimitResolver.Resolve(rawValue);

        Assert.NotEqual(SearchLimitResolver.MaxLimit, result.EffectiveLimit);
        Assert.NotEqual(SearchLimitResolver.MinLimit, result.EffectiveLimit);
        Assert.Equal(SearchLimitResolver.DefaultLimit, result.EffectiveLimit);
    }

    // AC-4: причина отказа называет сырое значение — оно попадает в Warning клиента.
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(11)]
    public void Resolve_FallbackReason_MentionsRawValue(int rawValue)
    {
        var result = SearchLimitResolver.Resolve(rawValue);

        Assert.NotNull(result.FallbackReason);
        Assert.Contains(rawValue.ToString(System.Globalization.CultureInfo.InvariantCulture), result.FallbackReason, StringComparison.Ordinal);
    }

    // AC-4: инвариант — причина есть только при fallback.
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(11)]
    public void Resolve_FallbackReasonPresent_OnlyWhenFallbackUsed(int rawValue)
    {
        var result = SearchLimitResolver.Resolve(rawValue);

        Assert.Equal(result.UsedFallback, result.FallbackReason is not null);
    }

    // AC-4: Resolve не бросает исключений ни на одном входе, включая крайние значения int.
    [Fact]
    public void Resolve_NeverThrows_ForAnyInput()
    {
        var inputs = new[]
        {
            int.MinValue,
            int.MinValue + 1,
            -1,
            0,
            1,
            2,
            3,
            9,
            10,
            11,
            int.MaxValue - 1,
            int.MaxValue,
        };

        foreach (var rawValue in inputs)
        {
            var result = default(ResolvedSearchLimit);

            var exception = Record.Exception(() => result = SearchLimitResolver.Resolve(rawValue));

            Assert.True(exception is null, $"Resolve бросил {exception?.GetType().Name} на входе {rawValue}");
            Assert.InRange(result.EffectiveLimit, 1, 10);
            Assert.Equal(result.UsedFallback, result.FallbackReason is not null);
        }
    }
}
