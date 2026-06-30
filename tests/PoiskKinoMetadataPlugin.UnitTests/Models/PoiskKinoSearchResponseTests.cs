using System.Text.Json;
using PoiskKinoMetadataPlugin.UnitTests.TestData;

namespace PoiskKinoMetadataPlugin.UnitTests.Models;

public class PoiskKinoSearchResponseTests
{
    private static JsonSerializerOptions Options => new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Deserialize_SearchResponse_ReturnsCorrectData()
    {
        var result = System.Text.Json.JsonSerializer.Deserialize<PoiskKinoMetadataPlugin.Models.PoiskKinoSearchResponse>(
            TestJsonData.SearchResponseJson, Options);

        Assert.NotNull(result);
        Assert.Equal(2, result.Total);
        Assert.Equal(10, result.Limit);
        Assert.Equal(1, result.Page);
        Assert.Equal(1, result.Pages);
    }

    [Fact]
    public void Deserialize_SearchResponse_DocsContainMovieAndSeries()
    {
        var result = System.Text.Json.JsonSerializer.Deserialize<PoiskKinoMetadataPlugin.Models.PoiskKinoSearchResponse>(
            TestJsonData.SearchResponseJson, Options);

        Assert.NotNull(result!.Docs);
        Assert.Equal(2, result.Docs.Count);

        var movie = result.Docs[0];
        Assert.Equal(535341, movie.Id);
        Assert.Equal("Оппенгеймер", movie.Name);
        Assert.Equal("movie", movie.Type);
        Assert.False(movie.IsSeries);

        var series = result.Docs[1];
        Assert.Equal(123456, series.Id);
        Assert.Equal("Во все тяжкие", series.Name);
        Assert.Equal("tv-series", series.Type);
        Assert.True(series.IsSeries);
    }

    [Fact]
    public void Deserialize_SearchResponse_ResultsPropertyReturnsDocs()
    {
        var result = System.Text.Json.JsonSerializer.Deserialize<PoiskKinoMetadataPlugin.Models.PoiskKinoSearchResponse>(
            TestJsonData.SearchResponseJson, Options);

        Assert.NotNull(result!.Results);
        Assert.Equal(result.Docs?.Count, result.Results?.Count);
        Assert.Same(result.Docs, result.Results);
    }

    [Fact]
    public void Deserialize_EmptySearchResponse_ReturnsEmptyDocs()
    {
        var result = System.Text.Json.JsonSerializer.Deserialize<PoiskKinoMetadataPlugin.Models.PoiskKinoSearchResponse>(
            TestJsonData.EmptySearchResponseJson, Options);

        Assert.NotNull(result);
        Assert.Equal(0, result.Total);
        Assert.Equal(0, result.Pages);
        Assert.NotNull(result.Docs);
        Assert.Empty(result.Docs);
    }

    [Fact]
    public void Deserialize_SearchResponse_PosterIsCorrect()
    {
        var result = System.Text.Json.JsonSerializer.Deserialize<PoiskKinoMetadataPlugin.Models.PoiskKinoSearchResponse>(
            TestJsonData.SearchResponseJson, Options);

        var movie = result!.Docs![0];
        Assert.NotNull(movie.Poster);
        Assert.Contains("oppenheimer-poster.jpg", movie.Poster.Url);
    }

    [Fact]
    public void Deserialize_SearchResponse_RatingIsCorrect()
    {
        var result = System.Text.Json.JsonSerializer.Deserialize<PoiskKinoMetadataPlugin.Models.PoiskKinoSearchResponse>(
            TestJsonData.SearchResponseJson, Options);

        var movie = result!.Docs![0];
        Assert.NotNull(movie.Rating);
        Assert.Equal(8.6, movie.Rating.Kp);
        Assert.Equal(8.4, movie.Rating.Imdb);
        Assert.Equal(8.1, movie.Rating.Tmdb);
    }

    [Fact]
    public void Deserialize_SearchResponse_ExternalIdIsCorrect()
    {
        var result = System.Text.Json.JsonSerializer.Deserialize<PoiskKinoMetadataPlugin.Models.PoiskKinoSearchResponse>(
            TestJsonData.SearchResponseJson, Options);

        var movie = result!.Docs![0];
        Assert.NotNull(movie.ExternalId);
        Assert.Equal("tt15398776", movie.ExternalId.Imdb);
        Assert.Equal(872585, movie.ExternalId.Tmdb);
    }
}
