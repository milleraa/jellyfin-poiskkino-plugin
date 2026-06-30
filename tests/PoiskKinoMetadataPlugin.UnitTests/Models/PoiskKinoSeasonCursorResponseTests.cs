using System.Text.Json;
using PoiskKinoMetadataPlugin.UnitTests.TestData;

namespace PoiskKinoMetadataPlugin.UnitTests.Models;

public class PoiskKinoSeasonCursorResponseTests
{
    private static JsonSerializerOptions Options => new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Deserialize_SeasonCursorResponse_ReturnsCorrectPagination()
    {
        var result = System.Text.Json.JsonSerializer.Deserialize<PoiskKinoMetadataPlugin.Models.PoiskKinoSeasonCursorResponse>(
            TestJsonData.SeasonCursorResponseJson, Options);

        Assert.NotNull(result);
        Assert.Equal(1, result.Limit);
        Assert.Equal(1, result.Total);
        Assert.Equal("cursor_next_page", result.Next);
        Assert.Null(result.Prev);
        Assert.False(result.HasNext);
        Assert.False(result.HasPrev);
    }

    [Fact]
    public void Deserialize_SeasonCursorResponse_DocsContainSeason()
    {
        var result = System.Text.Json.JsonSerializer.Deserialize<PoiskKinoMetadataPlugin.Models.PoiskKinoSeasonCursorResponse>(
            TestJsonData.SeasonCursorResponseJson, Options);

        Assert.NotNull(result!.Docs);
        Assert.Single(result.Docs);
        Assert.Equal(535341, result.Docs[0].MovieId);
        Assert.Equal(1, result.Docs[0].Number);
        Assert.Equal("Сезон 1", result.Docs[0].Name);
        Assert.Equal("Season 1", result.Docs[0].EnName);
        Assert.Equal(480, result.Docs[0].Duration);
    }

    [Fact]
    public void Deserialize_SeasonCursorResponse_EpisodesAreCorrect()
    {
        var result = System.Text.Json.JsonSerializer.Deserialize<PoiskKinoMetadataPlugin.Models.PoiskKinoSeasonCursorResponse>(
            TestJsonData.SeasonCursorResponseJson, Options);

        var season = result!.Docs![0];
        Assert.NotNull(season.Episodes);
        Assert.Equal(3, season.Episodes.Count);
        Assert.Equal(8, season.EpisodesCount);

        var ep1 = season.Episodes[0];
        Assert.Equal(1, ep1.Number);
        Assert.Equal("Пилот", ep1.Name);
        Assert.Equal("Pilot", ep1.EnName);
        Assert.NotNull(ep1.Still);
        Assert.Contains("ep1.jpg", ep1.Still.Url);

        var ep2 = season.Episodes[1];
        Assert.Equal(2, ep2.Number);
        Assert.Null(ep2.Still);

        var ep3 = season.Episodes[2];
        Assert.Equal(3, ep3.Number);
        Assert.Null(ep3.AirDate);
    }

    [Fact]
    public void Deserialize_SeasonCursorResponse_EpisodeDateFallbackWorks()
    {
        var result = System.Text.Json.JsonSerializer.Deserialize<PoiskKinoMetadataPlugin.Models.PoiskKinoSeasonCursorResponse>(
            TestJsonData.SeasonCursorResponseJson, Options);

        var ep1 = result!.Docs![0].Episodes![0];
        Assert.NotNull(ep1.Date);
        Assert.Equal(2023, ep1.Date!.Value.Year);

        var ep3 = result.Docs[0].Episodes![2];
        Assert.NotNull(ep3.Date);
        Assert.Equal(2023, ep3.Date!.Value.Year);
    }

    [Fact]
    public void Deserialize_SeasonCursorResponse_SeasonPosterIsCorrect()
    {
        var result = System.Text.Json.JsonSerializer.Deserialize<PoiskKinoMetadataPlugin.Models.PoiskKinoSeasonCursorResponse>(
            TestJsonData.SeasonCursorResponseJson, Options);

        var season = result!.Docs![0];
        Assert.NotNull(season.Poster);
        Assert.Contains("season1-poster.jpg", season.Poster.Url);
    }

    [Fact]
    public void Deserialize_EmptySeasonCursorResponse_ReturnsEmptyDocs()
    {
        var result = System.Text.Json.JsonSerializer.Deserialize<PoiskKinoMetadataPlugin.Models.PoiskKinoSeasonCursorResponse>(
            TestJsonData.EmptySeasonCursorResponseJson, Options);

        Assert.NotNull(result);
        Assert.NotNull(result.Docs);
        Assert.Empty(result.Docs);
        Assert.Null(result.Next);
        Assert.Null(result.Prev);
        Assert.False(result.HasNext);
        Assert.False(result.HasPrev);
    }
}
