namespace PoiskKinoMetadataPlugin.UnitTests.TestData;

public static class TestJsonData
{
    /// <summary>
    /// Full PoiskKinoMovieDtoV1_4 JSON matching GET /v1.4/movie/{id} response from API docs.
    /// </summary>
    public const string FullMovieDtoJson = """
    {
        "id": 535341,
        "name": "Оппенгеймер",
        "enName": "Oppenheimer",
        "alternativeName": "Oppenheimer",
        "type": "movie",
        "typeNumber": 1,
        "year": 2023,
        "description": "История жизни американского физика-теоретика Роберта Оппенгеймера...",
        "shortDescription": "История создателя атомной бомбы",
        "slogan": "The world forever changes",
        "status": "completed",
        "rating": {
            "kp": 8.6,
            "imdb": 8.4,
            "tmdb": 8.1,
            "filmCritics": 9.0,
            "russianFilmCritics": 8.9,
            "await": 7.2
        },
        "votes": {
            "kp": 650000,
            "imdb": 780000,
            "tmdb": 12000,
            "filmCritics": 500,
            "russianFilmCritics": 120,
            "await": 25000
        },
        "movieLength": 180,
        "isSeries": false,
        "ageRating": 18,
        "ratingMpaa": "R",
        "externalId": {
            "imdb": "tt15398776",
            "tmdb": 872585,
            "kpHD": "48e8d0acb0f62d8585101798eaeceec5"
        },
        "poster": {
            "url": "https://image.openmoviedb.com/kinopoisk-images/oppenheimer-poster.jpg",
            "previewUrl": "https://image.openmoviedb.com/kinopoisk-images/oppenheimer-poster-preview.jpg"
        },
        "backdrop": {
            "url": "https://image.openmoviedb.com/kinopoisk-images/oppenheimer-backdrop.jpg",
            "previewUrl": null
        },
        "logo": {
            "url": "https://image.openmoviedb.com/kinopoisk-images/oppenheimer-logo.png",
            "previewUrl": null
        },
        "genres": [
            { "name": "драма" },
            { "name": "биография" },
            { "name": "история" }
        ],
        "countries": [
            { "name": "США" },
            { "name": "Великобритания" }
        ],
        "persons": [
            {
                "id": 1234,
                "name": "Кристофер Нолан",
                "enName": "Christopher Nolan",
                "photo": "https://example.com/nolan.jpg",
                "description": "Director",
                "profession": "режиссер",
                "enProfession": "director"
            },
            {
                "id": 5678,
                "name": "Киллиан Мерфи",
                "enName": "Cillian Murphy",
                "photo": "https://example.com/murphy.jpg",
                "description": "Lead actor",
                "profession": "актер",
                "enProfession": "actor"
            }
        ],
        "videos": {
            "trailers": [
                {
                    "url": "https://www.youtube.com/watch?v=uYPbbksJxIg",
                    "site": "youtube",
                    "name": "Official Trailer",
                    "type": "TRAILER"
                }
            ]
        },
        "seasonsInfo": [],
        "budget": {
            "value": 100000000,
            "currency": "USD"
        },
        "fees": {
            "world": { "value": 975000000, "currency": "USD" },
            "russia": { "value": 45000000, "currency": "USD" },
            "usa": { "value": 329000000, "currency": "USD" }
        },
        "premiere": {
            "country": "США",
            "world": "2023-07-21T00:00:00.000Z",
            "russia": "2023-07-22T00:00:00.000Z",
            "digital": "2023-11-21T00:00:00.000Z",
            "cinema": "2023-07-21T00:00:00.000Z",
            "bluray": "2023-11-21T00:00:00.000Z",
            "dvd": "2023-11-21T00:00:00.000Z"
        },
        "similarMovies": [
            {
                "id": 12345,
                "name": "Интерстеллар",
                "enName": "Interstellar",
                "alternativeName": "Interstellar",
                "type": "movie",
                "poster": { "url": "https://image.openmoviedb.com/interstellar-poster.jpg", "previewUrl": null },
                "rating": { "kp": 8.8, "imdb": 8.7, "tmdb": 8.4 },
                "year": 2014
            }
        ],
        "sequelsAndPrequels": [],
        "watchability": {
            "items": [
                {
                    "name": "ivi",
                    "url": "https://www.ivi.ru/watch/oppenheimer",
                    "logo": { "url": "https://image.openmoviedb.com/ivi-logo.png", "previewUrl": null }
                }
            ]
        },
        "releaseYears": [ { "start": 2023, "end": 2023 } ],
        "top10": null,
        "top250": 25,
        "ticketsOnSale": false,
        "audience": [
            { "count": 12000000, "country": "Россия" },
            { "count": 76000000, "country": "США" }
        ],
        "lists": [ "top250", "top-100-action-movies" ],
        "networks": {
            "items": [
                {
                    "name": "Universal Pictures",
                    "logo": { "url": "https://image.openmoviedb.com/universal-logo.png", "previewUrl": null }
                }
            ]
        },
        "updatedAt": "2024-01-15T10:30:00.000Z",
        "createdAt": "2023-07-21T00:00:00.000Z"
    }
    """;

    /// <summary>
    /// Minimal movie JSON with only required fields.
    /// </summary>
    public const string MinimalMovieDtoJson = """
    {
        "id": 100,
        "name": "Test Movie",
        "year": 2000
    }
    """;

    /// <summary>
    /// PoiskKinoSearchResponse JSON matching v1.4 movie/search endpoint response.
    /// </summary>
    public const string SearchResponseJson = """
    {
        "docs": [
            {
                "id": 535341,
                "name": "Оппенгеймер",
                "enName": "Oppenheimer",
                "alternativeName": "Oppenheimer",
                "type": "movie",
                "year": 2023,
                "description": "История жизни американского физика-теоретика Роберта Оппенгеймера...",
                "shortDescription": "История создателя атомной бомбы",
                "movieLength": 180,
                "isSeries": false,
                "ageRating": 18,
                "ratingMpaa": "R",
                "externalId": {
                    "imdb": "tt15398776",
                    "tmdb": 872585,
                    "kpHD": "48e8d0acb0f62d8585101798eaeceec5"
                },
                "poster": {
                    "url": "https://image.openmoviedb.com/kinopoisk-images/oppenheimer-poster.jpg",
                    "previewUrl": "https://image.openmoviedb.com/kinopoisk-images/oppenheimer-poster-preview.jpg"
                },
                "backdrop": {
                    "url": "https://image.openmoviedb.com/kinopoisk-images/oppenheimer-backdrop.jpg",
                    "previewUrl": null
                },
                "rating": {
                    "kp": 8.6,
                    "imdb": 8.4,
                    "tmdb": 8.1
                },
                "votes": {
                    "kp": 650000,
                    "imdb": 780000,
                    "tmdb": 12000
                },
                "genres": [
                    { "name": "драма" },
                    { "name": "биография" }
                ],
                "countries": [
                    { "name": "США" }
                ],
                "top10": null,
                "top250": 25,
                "typeNumber": 1,
                "status": "completed",
                "ticketsOnSale": false
            },
            {
                "id": 123456,
                "name": "Во все тяжкие",
                "enName": "Breaking Bad",
                "type": "tv-series",
                "year": 2008,
                "description": "Школьный учитель химии Уолтер Уайт узнаёт, что болен раком лёгких...",
                "isSeries": true,
                "rating": {
                    "kp": 8.9,
                    "imdb": 9.5
                },
                "poster": {
                    "url": "https://image.openmoviedb.com/breaking-bad-poster.jpg"
                },
                "genres": [
                    { "name": "драма" },
                    { "name": "криминал" }
                ]
            }
        ],
        "total": 2,
        "limit": 10,
        "page": 1,
        "pages": 1
    }
    """;

    /// <summary>
    /// Empty search response.
    /// </summary>
    public const string EmptySearchResponseJson = """
    {
        "docs": [],
        "total": 0,
        "limit": 10,
        "page": 1,
        "pages": 0
    }
    """;

    /// <summary>
    /// PoiskKinoSeasonCursorResponse JSON matching v1.5/season? endpoint.
    /// </summary>
    public const string SeasonCursorResponseJson = """
    {
        "docs": [
            {
                "movieId": 535341,
                "number": 1,
                "episodesCount": 8,
                "episodes": [
                    {
                        "number": 1,
                        "name": "Пилот",
                        "enName": "Pilot",
                        "description": "Первый эпизод сезона",
                        "enDescription": "The first episode of the season",
                        "still": { "url": "https://example.com/ep1.jpg", "previewUrl": null },
                        "airDate": "2023-01-15T00:00:00.000Z",
                        "date": "2023-01-15T00:00:00.000Z"
                    },
                    {
                        "number": 2,
                        "name": "Эпизод 2",
                        "enName": "Episode 2",
                        "description": "Второй эпизод",
                        "still": null,
                        "airDate": "2023-01-22T00:00:00.000Z"
                    },
                    {
                        "number": 3,
                        "name": "Эпизод 3",
                        "enName": "Episode 3",
                        "description": "Третий эпизод",
                        "still": null,
                        "airDate": null,
                        "date": "2023-01-29T00:00:00.000Z"
                    }
                ],
                "poster": {
                    "url": "https://example.com/season1-poster.jpg",
                    "previewUrl": null
                },
                "name": "Сезон 1",
                "enName": "Season 1",
                "duration": 480,
                "description": "Первый сезон сериала",
                "enDescription": "First season of the series",
                "airDate": "2023-01-15T00:00:00.000Z",
                "updatedAt": "2024-01-15T10:30:00.000Z",
                "createdAt": "2023-01-01T00:00:00.000Z"
            }
        ],
        "total": 1,
        "limit": 1,
        "next": "cursor_next_page",
        "prev": null,
        "hasNext": false,
        "hasPrev": false
    }
    """;

    /// <summary>
    /// Empty season cursor response (no docs).
    /// </summary>
    public const string EmptySeasonCursorResponseJson = """
    {
        "docs": [],
        "limit": 10,
        "next": null,
        "prev": null,
        "hasNext": false,
        "hasPrev": false
    }
    """;

    /// <summary>
    /// Search response for series-only results.
    /// </summary>
    public const string SeriesSearchResponseJson = """
    {
        "docs": [
            {
                "id": 404900,
                "name": "Игра престолов",
                "enName": "Game of Thrones",
                "type": "tv-series",
                "year": 2011,
                "description": "Сериал о борьбе за власть в Семи Королевствах",
                "isSeries": true,
                "seriesLength": 60,
                "totalSeriesLength": 4320,
                "status": "completed",
                "externalId": {
                    "imdb": "tt0944947",
                    "tmdb": 1399,
                    "kpHD": "58e9b3c0a0f72d849a04801aeaeceec6"
                },
                "poster": {
                    "url": "https://image.openmoviedb.com/got-poster.jpg"
                },
                "rating": {
                    "kp": 9.1,
                    "imdb": 9.2
                },
                "genres": [
                    { "name": "фэнтези" },
                    { "name": "драма" }
                ],
                "releaseYears": [
                    { "start": 2011, "end": 2019 }
                ]
            }
        ],
        "total": 1,
        "limit": 10,
        "page": 1,
        "pages": 1
    }
    """;

    /// <summary>
    /// Mixed search response with both movies and series.
    /// </summary>
    public const string MixedSearchResponseJson = """
    {
        "docs": [
            {
                "id": 535341,
                "name": "Оппенгеймер",
                "isSeries": false,
                "year": 2023,
                "poster": { "url": "https://example.com/movie-poster.jpg" },
                "rating": { "kp": 8.6, "imdb": 8.4 }
            },
            {
                "id": 404900,
                "name": "Игра престолов",
                "isSeries": true,
                "year": 2011,
                "poster": { "url": "https://example.com/series-poster.jpg" },
                "rating": { "kp": 9.1, "imdb": 9.2 }
            },
            {
                "id": 100,
                "name": "Фильм без флага",
                "year": 2020,
                "poster": { "url": "https://example.com/unknown-poster.jpg" }
            }
        ],
        "total": 3,
        "limit": 10,
        "page": 1,
        "pages": 1
    }
    """;
}
