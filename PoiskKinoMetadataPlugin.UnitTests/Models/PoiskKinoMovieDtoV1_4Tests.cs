using System.Text.Json;
using PoiskKinoMetadataPlugin.Models;
using PoiskKinoMetadataPlugin.UnitTests.TestData;

namespace PoiskKinoMetadataPlugin.UnitTests.Models;

public class PoiskKinoMovieDtoV1_4Tests
{
    private static JsonSerializerOptions Options => new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Deserialize_FullMovieJson_ReturnsAllFields()
    {
        var result = JsonSerializer.Deserialize<PoiskKinoMovieDtoV1_4>(
            TestJsonData.FullMovieDtoJson, Options);

        Assert.NotNull(result);
        Assert.Equal(535341, result.Id);
        Assert.Equal("Оппенгеймер", result.Name);
        Assert.Equal("Oppenheimer", result.EnName);
        Assert.Equal("Oppenheimer", result.AlternativeName);
        Assert.Equal("movie", result.Type);
        Assert.Equal(1, result.TypeNumber);
        Assert.Equal(2023, result.Year);
        Assert.Contains("Роберта Оппенгеймера", result.Description);
        Assert.Equal("История создателя атомной бомбы", result.ShortDescription);
        Assert.Equal("The world forever changes", result.Slogan);
        Assert.Equal("completed", result.Status);
        Assert.Equal(180, result.MovieLength);
        Assert.False(result.IsSeries);
        Assert.Equal(18, result.AgeRating);
        Assert.Equal("R", result.RatingMpaa);
        Assert.Equal(25, result.Top250);
        Assert.Null(result.Top10);
        Assert.False(result.TicketsOnSale);
    }

    [Fact]
    public void Deserialize_FullMovieJson_RatingIsCorrect()
    {
        var result = JsonSerializer.Deserialize<PoiskKinoMovieDtoV1_4>(
            TestJsonData.FullMovieDtoJson, Options);

        Assert.NotNull(result!.Rating);
        Assert.Equal(8.6, result.Rating.Kp);
        Assert.Equal(8.4, result.Rating.Imdb);
        Assert.Equal(8.1, result.Rating.Tmdb);
        Assert.Equal(9.0, result.Rating.FilmCritics);
        Assert.Equal(8.9, result.Rating.RussianFilmCritics);
        Assert.Equal(7.2, result.Rating.Await);
    }

    [Fact]
    public void Deserialize_FullMovieJson_VotesAreCorrect()
    {
        var result = JsonSerializer.Deserialize<PoiskKinoMovieDtoV1_4>(
            TestJsonData.FullMovieDtoJson, Options);

        Assert.NotNull(result!.Votes);
        Assert.Equal(650000, result.Votes.Kp);
        Assert.Equal(780000, result.Votes.Imdb);
        Assert.Equal(12000, result.Votes.Tmdb);
        Assert.Equal(500, result.Votes.FilmCritics);
        Assert.Equal(120, result.Votes.RussianFilmCritics);
        Assert.Equal(25000, result.Votes.Await);
    }

    [Fact]
    public void Deserialize_FullMovieJson_ExternalIdIsCorrect()
    {
        var result = JsonSerializer.Deserialize<PoiskKinoMovieDtoV1_4>(
            TestJsonData.FullMovieDtoJson, Options);

        Assert.NotNull(result!.ExternalId);
        Assert.Equal("tt15398776", result.ExternalId.Imdb);
        Assert.Equal(872585, result.ExternalId.Tmdb);
        Assert.Equal("48e8d0acb0f62d8585101798eaeceec5", result.ExternalId.KpHd);
    }

    [Fact]
    public void Deserialize_FullMovieJson_PosterAndBackdropAreCorrect()
    {
        var result = JsonSerializer.Deserialize<PoiskKinoMovieDtoV1_4>(
            TestJsonData.FullMovieDtoJson, Options);

        Assert.NotNull(result!.Poster);
        Assert.Contains("oppenheimer-poster.jpg", result.Poster.Url);
        Assert.Contains("oppenheimer-poster-preview.jpg", result.Poster.PreviewUrl);
        Assert.NotNull(result.Backdrop);
        Assert.Contains("oppenheimer-backdrop.jpg", result.Backdrop.Url);
        Assert.Null(result.Backdrop.PreviewUrl);
    }

    [Fact]
    public void Deserialize_FullMovieJson_LogoIsCorrect()
    {
        var result = JsonSerializer.Deserialize<PoiskKinoMovieDtoV1_4>(
            TestJsonData.FullMovieDtoJson, Options);

        Assert.NotNull(result!.Logo);
        Assert.Contains("oppenheimer-logo.png", result.Logo.Url);
    }

    [Fact]
    public void Deserialize_FullMovieJson_GenresAreCorrect()
    {
        var result = JsonSerializer.Deserialize<PoiskKinoMovieDtoV1_4>(
            TestJsonData.FullMovieDtoJson, Options);

        Assert.NotNull(result!.Genres);
        Assert.Equal(3, result.Genres.Count);
        Assert.Equal("драма", result.Genres[0].Name);
        Assert.Equal("биография", result.Genres[1].Name);
        Assert.Equal("история", result.Genres[2].Name);
    }

    [Fact]
    public void Deserialize_FullMovieJson_CountriesAreCorrect()
    {
        var result = JsonSerializer.Deserialize<PoiskKinoMovieDtoV1_4>(
            TestJsonData.FullMovieDtoJson, Options);

        Assert.NotNull(result!.Countries);
        Assert.Equal(2, result.Countries.Count);
        Assert.Equal("США", result.Countries[0].Name);
        Assert.Equal("Великобритания", result.Countries[1].Name);
    }

    [Fact]
    public void Deserialize_FullMovieJson_PersonsAreCorrect()
    {
        var result = JsonSerializer.Deserialize<PoiskKinoMovieDtoV1_4>(
            TestJsonData.FullMovieDtoJson, Options);

        Assert.NotNull(result!.Persons);
        Assert.Equal(2, result.Persons.Count);
        Assert.Equal("Кристофер Нолан", result.Persons[0].Name);
        Assert.Equal("режиссер", result.Persons[0].Profession);
        Assert.Equal("director", result.Persons[0].EnProfession);
        Assert.Equal("Киллиан Мерфи", result.Persons[1].Name);
        Assert.Equal("актер", result.Persons[1].Profession);
    }

    [Fact]
    public void Deserialize_FullMovieJson_VideosTrailersAreCorrect()
    {
        var result = JsonSerializer.Deserialize<PoiskKinoMovieDtoV1_4>(
            TestJsonData.FullMovieDtoJson, Options);

        Assert.NotNull(result!.Videos);
        Assert.NotNull(result.Videos.Trailers);
        Assert.Single(result.Videos.Trailers);
        Assert.Contains("youtube", result.Videos.Trailers[0].Url);
        Assert.Equal("youtube", result.Videos.Trailers[0].Site);
        Assert.Equal("Official Trailer", result.Videos.Trailers[0].Name);
    }

    [Fact]
    public void Deserialize_FullMovieJson_SeasonsInfoIsEmptyArray()
    {
        var result = JsonSerializer.Deserialize<PoiskKinoMovieDtoV1_4>(
            TestJsonData.FullMovieDtoJson, Options);

        Assert.NotNull(result!.SeasonsInfo);
        Assert.Empty(result.SeasonsInfo);
    }

    [Fact]
    public void Deserialize_FullMovieJson_BudgetAndFeesAreCorrect()
    {
        var result = JsonSerializer.Deserialize<PoiskKinoMovieDtoV1_4>(
            TestJsonData.FullMovieDtoJson, Options);

        Assert.NotNull(result!.Budget);
        Assert.Equal(100000000, result.Budget.Value);
        Assert.Equal("USD", result.Budget.Currency);

        Assert.NotNull(result.Fees);
        Assert.NotNull(result.Fees.World);
        Assert.Equal(975000000, result.Fees.World.Value);
        Assert.NotNull(result.Fees.Russia);
        Assert.Equal(45000000, result.Fees.Russia.Value);
        Assert.NotNull(result.Fees.Usa);
        Assert.Equal(329000000, result.Fees.Usa.Value);
    }

    [Fact]
    public void Deserialize_FullMovieJson_PremiereIsCorrect()
    {
        var result = JsonSerializer.Deserialize<PoiskKinoMovieDtoV1_4>(
            TestJsonData.FullMovieDtoJson, Options);

        Assert.NotNull(result!.Premiere);
        Assert.Equal("США", result.Premiere.Country);
        Assert.Contains("2023-07-21", result.Premiere.World);
    }

    [Fact]
    public void Deserialize_FullMovieJson_SimilarMoviesAreCorrect()
    {
        var result = JsonSerializer.Deserialize<PoiskKinoMovieDtoV1_4>(
            TestJsonData.FullMovieDtoJson, Options);

        Assert.NotNull(result!.SimilarMovies);
        Assert.Single(result.SimilarMovies);
        Assert.Equal(12345, result.SimilarMovies[0].Id);
        Assert.Equal("Интерстеллар", result.SimilarMovies[0].Name);
        Assert.NotNull(result.SimilarMovies[0].Rating);
        Assert.Equal(8.8, result.SimilarMovies[0].Rating!.Kp);
    }

    [Fact]
    public void Deserialize_FullMovieJson_SequelsAndPrequelsIsEmptyArray()
    {
        var result = JsonSerializer.Deserialize<PoiskKinoMovieDtoV1_4>(
            TestJsonData.FullMovieDtoJson, Options);

        Assert.NotNull(result!.SequelsAndPrequels);
        Assert.Empty(result.SequelsAndPrequels);
    }

    [Fact]
    public void Deserialize_FullMovieJson_WatchabilityIsCorrect()
    {
        var result = JsonSerializer.Deserialize<PoiskKinoMovieDtoV1_4>(
            TestJsonData.FullMovieDtoJson, Options);

        Assert.NotNull(result!.Watchability);
        Assert.NotNull(result.Watchability.Items);
        Assert.Single(result.Watchability.Items);
        Assert.Equal("ivi", result.Watchability.Items[0].Name);
        Assert.NotNull(result.Watchability.Items[0].Logo);
    }

    [Fact]
    public void Deserialize_FullMovieJson_ReleaseYearsAreCorrect()
    {
        var result = JsonSerializer.Deserialize<PoiskKinoMovieDtoV1_4>(
            TestJsonData.FullMovieDtoJson, Options);

        Assert.NotNull(result!.ReleaseYears);
        Assert.Single(result.ReleaseYears);
        Assert.Equal(2023, result.ReleaseYears[0].Start);
        Assert.Equal(2023, result.ReleaseYears[0].End);
    }

    [Fact]
    public void Deserialize_FullMovieJson_AudienceIsCorrect()
    {
        var result = JsonSerializer.Deserialize<PoiskKinoMovieDtoV1_4>(
            TestJsonData.FullMovieDtoJson, Options);

        Assert.NotNull(result!.Audience);
        Assert.Equal(2, result.Audience.Count);
        Assert.Equal(12000000, result.Audience[0].Count);
        Assert.Equal("Россия", result.Audience[0].Country);
        Assert.Equal(76000000, result.Audience[1].Count);
    }

    [Fact]
    public void Deserialize_FullMovieJson_ListsAreCorrect()
    {
        var result = JsonSerializer.Deserialize<PoiskKinoMovieDtoV1_4>(
            TestJsonData.FullMovieDtoJson, Options);

        Assert.NotNull(result!.Lists);
        Assert.Equal(2, result.Lists.Count);
        Assert.Contains("top250", result.Lists);
        Assert.Contains("top-100-action-movies", result.Lists);
    }

    [Fact]
    public void Deserialize_FullMovieJson_NetworksIsCorrect()
    {
        var result = JsonSerializer.Deserialize<PoiskKinoMovieDtoV1_4>(
            TestJsonData.FullMovieDtoJson, Options);

        Assert.NotNull(result!.Networks);
        Assert.NotNull(result.Networks.Items);
        Assert.Single(result.Networks.Items);
        Assert.Equal("Universal Pictures", result.Networks.Items[0].Name);
    }

    [Fact]
    public void Deserialize_FullMovieJson_TimestampsAreCorrect()
    {
        var result = JsonSerializer.Deserialize<PoiskKinoMovieDtoV1_4>(
            TestJsonData.FullMovieDtoJson, Options);

        Assert.NotNull(result!.UpdatedAt);
        Assert.Equal(2024, result.UpdatedAt.Value.Year);
        Assert.NotNull(result.CreatedAt);
        Assert.Equal(2023, result.CreatedAt.Value.Year);
    }

    [Fact]
    public void Deserialize_MinimalMovieJson_ReturnsNullForMissingFields()
    {
        var result = JsonSerializer.Deserialize<PoiskKinoMovieDtoV1_4>(
            TestJsonData.MinimalMovieDtoJson, Options);

        Assert.NotNull(result);
        Assert.Equal(100, result.Id);
        Assert.Equal("Test Movie", result.Name);
        Assert.Equal(2000, result.Year);
        Assert.Null(result.EnName);
        Assert.Null(result.Description);
        Assert.Null(result.Rating);
        Assert.Null(result.Votes);
        Assert.Null(result.ExternalId);
        Assert.Null(result.Poster);
        Assert.Null(result.Backdrop);
        Assert.Null(result.Genres);
        Assert.Null(result.Persons);
    }
}
