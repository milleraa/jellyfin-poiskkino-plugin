using Microsoft.Extensions.Logging;
using Moq;
using PoiskKinoMetadataPlugin.Models;

namespace PoiskKinoMetadataPlugin.UnitTests;

public class SearchYearFilterTests
{
    private readonly Mock<ILogger> _loggerMock = new();

    private static PoiskKinoItem Movie(int? year) => new() { Year = year };

    private static PoiskKinoItem Series(int? year, params PoiskKinoYearRange[] releaseYears) =>
        new() { Year = year, ReleaseYears = releaseYears.ToList() };

    [Theory]
    [InlineData(2023, 2023, true)]
    [InlineData(2023, 2024, false)]
    [InlineData(null, 2023, false)]
    public void MatchesMovie_ExactYearComparison(int? itemYear, int searchYear, bool expected)
    {
        Assert.Equal(expected, SearchYearFilter.MatchesMovie(Movie(itemYear), searchYear));
    }

    [Fact]
    public void MatchesSeries_YearWithinReleaseYears_ReturnsTrue()
    {
        var item = Series(2011, new PoiskKinoYearRange { Start = 2011, End = 2019 });

        Assert.True(SearchYearFilter.MatchesSeries(item, 2015));
    }

    [Fact]
    public void MatchesSeries_YearOutsideReleaseYears_ReturnsFalse()
    {
        var item = Series(2011, new PoiskKinoYearRange { Start = 2011, End = 2019 });

        Assert.False(SearchYearFilter.MatchesSeries(item, 2020));
    }

    [Fact]
    public void MatchesSeries_NoReleaseYears_MatchesStartYear()
    {
        var item = Series(2008);

        Assert.True(SearchYearFilter.MatchesSeries(item, 2008));
        Assert.False(SearchYearFilter.MatchesSeries(item, 2013));
    }

    [Fact]
    public void MatchesSeries_OpenEndedRange_CoversBoundary()
    {
        var item = Series(null, new PoiskKinoYearRange { Start = 2019 });

        Assert.True(SearchYearFilter.MatchesSeries(item, 2025));
        Assert.False(SearchYearFilter.MatchesSeries(item, 2018));
    }

    [Fact]
    public void Apply_NoYear_ReturnsUnfiltered()
    {
        var items = new List<PoiskKinoItem> { Movie(2000), Movie(2020) };

        var result = SearchYearFilter.Apply(items, null, SearchYearFilter.MatchesMovie, _loggerMock.Object);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Apply_MatchesExist_ReturnsOnlyMatchingItems()
    {
        var items = new List<PoiskKinoItem> { Movie(2023), Movie(1999), Movie(2023) };

        var result = SearchYearFilter.Apply(items, 2023, SearchYearFilter.MatchesMovie, _loggerMock.Object);

        Assert.Equal(2, result.Count);
        Assert.All(result, i => Assert.Equal(2023, i.Year));
    }

    [Fact]
    public void Apply_FilterEmptiesList_FallsBackToUnfilteredAndLogs()
    {
        var items = new List<PoiskKinoItem> { Movie(2023), Movie(2008) };

        var result = SearchYearFilter.Apply(items, 1999, SearchYearFilter.MatchesMovie, _loggerMock.Object);

        Assert.Same(items, result);
        Assert.Equal(2, result.Count);
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, _) => true),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Apply_EmptyInput_ReturnsEmptyWithoutFallbackLog()
    {
        var items = new List<PoiskKinoItem>();

        var result = SearchYearFilter.Apply(items, 2023, SearchYearFilter.MatchesMovie, _loggerMock.Object);

        Assert.Empty(result);
        _loggerMock.Verify(
            l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, _) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
