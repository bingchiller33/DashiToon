using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using DashiToon.Api.Domain.Exceptions;
using FluentAssertions;

namespace Domain.UnitTests;

public class SeriesTests
{
    [Fact]
    public void CreateNewSeriesShouldCreateSuccessfully()
    {
        // Arrange
        string title = "TestSeries";
        string synopsis = "TestSynopsis";
        string thumbnail = "TestThumbnailUrl";
        SeriesType type = SeriesType.Comic;
        Genre[] genres = [new("TestGenre", "TestGenreDescription")];
        CategoryRating[] categoryRatings =
        [
            CategoryRating.Create(ContentCategory.Violent, 1), CategoryRating.Create(ContentCategory.Nudity, 1),
            CategoryRating.Create(ContentCategory.Sexual, 0), CategoryRating.Create(ContentCategory.Profanity, 3),
            CategoryRating.Create(ContentCategory.Alcohol, 2), CategoryRating.Create(ContentCategory.Sensitive, 1)
        ];
        string[]? alternativeTitles = new[] { "Alter 1", "Alter 2" };
        string[]? authors = new[] { "Author 1" };
        DateTimeOffset startTime = DateTimeOffset.UtcNow;

        // Act
        Series series = Series.CreateNew(
            title,
            synopsis,
            thumbnail,
            type,
            genres,
            categoryRatings,
            alternativeTitles,
            authors,
            startTime);

        // Assert
        series.Should().NotBeNull();
        series.Status.Should().Be(SeriesStatus.Draft);
        series.Title.Should().Be(title);
        series.Synopsis.Should().Be(synopsis);
        series.Thumbnail.Should().Be(thumbnail);
        series.Type.Should().Be(type);
        series.ContentRating.Should().Be(ContentRating.Mature);
        series.AlternativeTitles.Should().BeEquivalentTo(alternativeTitles);
        series.Authors.Should().BeEquivalentTo(authors);
        series.StartTime.Should().BeWithin(TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public void UpdateSeriesShouldUpdateSuccessfully()
    {
        // Arrange
        Series series = Series.CreateNew(
            "TestSeries",
            "TestSynopsis",
            "TestThumbnailUrl",
            SeriesType.Comic,
            [new Genre("TestGenre", "TestGenreDescription")],
            [
                CategoryRating.Create(ContentCategory.Violent, 1), CategoryRating.Create(ContentCategory.Nudity, 1),
                CategoryRating.Create(ContentCategory.Sexual, 0), CategoryRating.Create(ContentCategory.Profanity, 3),
                CategoryRating.Create(ContentCategory.Alcohol, 2), CategoryRating.Create(ContentCategory.Sensitive, 1)
            ]);

        // Act
        series.Update(
            "UpdatedTitle",
            "UpdatedSynopsis",
            "UpdatedThumbnail",
            SeriesStatus.Hiatus,
            [new Genre("Genre1", "Genre1")],
            [
                CategoryRating.Create(ContentCategory.Violent, 1), CategoryRating.Create(ContentCategory.Nudity, 1),
                CategoryRating.Create(ContentCategory.Sexual, 2), CategoryRating.Create(ContentCategory.Profanity, 2),
                CategoryRating.Create(ContentCategory.Alcohol, 2), CategoryRating.Create(ContentCategory.Sensitive, 1)
            ],
            ["Alt1", "Alt2"],
            ["Auth 1", "Auth 2"],
            DateTimeOffset.UtcNow);

        // Assert
        series.Title.Should().Be("UpdatedTitle");
        series.Synopsis.Should().Be("UpdatedSynopsis");
        series.Thumbnail.Should().Be("UpdatedThumbnail");
        series.Status.Should().Be(SeriesStatus.Hiatus);
        series.Genres.Should().BeEquivalentTo([new Genre("Genre1", "Genre1")]);
        series.ContentRating.Should().Be(ContentRating.YoungAdult);
        series.AlternativeTitles.Should().BeEquivalentTo(["Alt1", "Alt2"]);
        series.Authors.Should().BeEquivalentTo(["Auth 1", "Auth 2"]);
        series.StartTime.Should().BeWithin(TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public void UpdateSeriesShouldSetStartTimeIfStatusChangeToOngoing()
    {
        // Arrange
        Series series = Series.CreateNew(
            "TestSeries",
            "TestSynopsis",
            "TestThumbnailUrl",
            SeriesType.Comic,
            [new Genre("TestGenre", "TestGenreDescription")],
            [
                CategoryRating.Create(ContentCategory.Violent, 1), CategoryRating.Create(ContentCategory.Nudity, 1),
                CategoryRating.Create(ContentCategory.Sexual, 0), CategoryRating.Create(ContentCategory.Profanity, 3),
                CategoryRating.Create(ContentCategory.Alcohol, 2), CategoryRating.Create(ContentCategory.Sensitive, 1)
            ]);

        // Act
        series.Update(
            "UpdatedTitle",
            "UpdatedSynopsis",
            "UpdatedThumbnail",
            SeriesStatus.Ongoing,
            [new Genre("Genre1", "Genre1")],
            [
                CategoryRating.Create(ContentCategory.Violent, 1), CategoryRating.Create(ContentCategory.Nudity, 1),
                CategoryRating.Create(ContentCategory.Sexual, 2), CategoryRating.Create(ContentCategory.Profanity, 2),
                CategoryRating.Create(ContentCategory.Alcohol, 2), CategoryRating.Create(ContentCategory.Sensitive, 1)
            ]);

        // Assert
        series.Title.Should().Be("UpdatedTitle");
        series.Synopsis.Should().Be("UpdatedSynopsis");
        series.Thumbnail.Should().Be("UpdatedThumbnail");
        series.Status.Should().Be(SeriesStatus.Ongoing);
        series.Genres.Should().BeEquivalentTo([new Genre("Genre1", "Genre1")]);
        series.ContentRating.Should().Be(ContentRating.YoungAdult);
        series.StartTime.Should().BeWithin(TimeSpan.FromMilliseconds(100));
    }

    [Theory]
    [MemberData(nameof(CreateNewSeriesShouldRequireValidTitleTestCases))]
    public void CreateNewSeriesShouldRequireValidTitle(string title)
    {
        // Arrange
        string synopsis = "TestSeries";
        string thumbnail = "TestSeries";
        SeriesType type = SeriesType.Comic;
        Genre[] genres = [];
        CategoryRating[] categoryRatings = [];

        // Act & Assert
        FluentActions
            .Invoking(() => Series.CreateNew(title, synopsis, thumbnail, type, genres, categoryRatings))
            .Should().Throw<ArgumentException>();
    }

    public static IEnumerable<object[]> CreateNewSeriesShouldRequireValidTitleTestCases()
    {
        return
        [
            [
                string.Empty
            ],
            [
                new string('*', 256)
            ]
        ];
    }

    [Theory]
    [MemberData(nameof(CreateNewSeriesShouldRequireValidSynopsisTestCases))]
    public void CreateNewSeriesShouldRequireValidSynopsis(string synopsis)
    {
        // Arrange
        string title = "TestSeries";
        string? thumbnail = "TestSeries";
        SeriesType type = SeriesType.Comic;
        Genre[] genres = [];
        CategoryRating[] categoryRatings = [];

        // Act & Assert
        FluentActions
            .Invoking(() => Series.CreateNew(title, synopsis, thumbnail, type, genres, categoryRatings))
            .Should().Throw<ArgumentException>();
    }

    public static IEnumerable<object[]> CreateNewSeriesShouldRequireValidSynopsisTestCases()
    {
        return
        [
            [
                string.Empty
            ],
            [
                new string('*', 5001)
            ]
        ];
    }

    [Theory]
    [MemberData(nameof(CreateNewSeriesShouldRequireValidGenresTestCases))]
    public void CreateNewSeriesShouldRequireValidGenres(Genre[] genres)
    {
        // Arrange
        string title = "TestTitle";
        string synopsis = "TestSeries";
        string? thumbnail = "TestSeries";
        SeriesType type = SeriesType.Comic;
        CategoryRating[] categoryRatings = [];

        // Act & Assert
        FluentActions
            .Invoking(() => Series.CreateNew(title, synopsis, thumbnail, type, genres, categoryRatings))
            .Should().Throw<ArgumentException>();
    }

    public static IEnumerable<object[]> CreateNewSeriesShouldRequireValidGenresTestCases()
    {
        return
        [
            [
                Array.Empty<Genre>()
            ],
            [
                new Genre[]
                {
                    new("", ""), new("", ""), new("", ""), new("", ""), new("", ""), new("", ""), new("", ""),
                    new("", ""), new("", ""), new("", ""), new("", ""), new("", ""), new("", "")
                }
            ]
        ];
    }

    [Theory]
    [MemberData(nameof(AllAgesTestCase))]
    public void EvaluateContentShouldReturnAllAges(CategoryRating[] categoryRatings)
    {
        // Act
        string title = "TestSeries";
        string synopsis = "TestSeries";
        string? thumbnail = "TestSeries";
        SeriesType type = SeriesType.Comic;
        Genre[] genres = [new("TestGenre", "TestDes")];

        // Act
        Series series = Series.CreateNew(title, synopsis, thumbnail, type, genres, categoryRatings);

        // Assert
        series.Should().NotBeNull();
        series.ContentRating.Should().Be(ContentRating.AllAges);
    }

    public static IEnumerable<object[]> AllAgesTestCase()
    {
        return
        [
            [
                new[]
                {
                    CategoryRating.Create(ContentCategory.Violent, 0),
                    CategoryRating.Create(ContentCategory.Nudity, 0),
                    CategoryRating.Create(ContentCategory.Sexual, 0),
                    CategoryRating.Create(ContentCategory.Profanity, 0),
                    CategoryRating.Create(ContentCategory.Alcohol, 0),
                    CategoryRating.Create(ContentCategory.Sensitive, 0)
                }
            ]
        ];
    }

    [Theory]
    [MemberData(nameof(TeenTestCase))]
    public void EvaluateContentShouldReturnTeen(CategoryRating[] categoryRatings)
    {
        // Act
        string title = "TestSeries";
        string synopsis = "TestSeries";
        string? thumbnail = "TestSeries";
        SeriesType type = SeriesType.Comic;
        Genre[] genres = [new("TestGenre", "TestDes")];

        // Act
        Series series = Series.CreateNew(title, synopsis, thumbnail, type, genres, categoryRatings);

        // Assert
        series.Should().NotBeNull();
        series.ContentRating.Should().Be(ContentRating.Teen);
    }

    public static IEnumerable<object[]> TeenTestCase()
    {
        return
        [
            [
                new[]
                {
                    CategoryRating.Create(ContentCategory.Violent, 1),
                    CategoryRating.Create(ContentCategory.Nudity, 1),
                    CategoryRating.Create(ContentCategory.Sexual, 1),
                    CategoryRating.Create(ContentCategory.Profanity, 1),
                    CategoryRating.Create(ContentCategory.Alcohol, 1),
                    CategoryRating.Create(ContentCategory.Sensitive, 1)
                }
            ],
            [
                new[]
                {
                    CategoryRating.Create(ContentCategory.Violent, 0),
                    CategoryRating.Create(ContentCategory.Nudity, 0),
                    CategoryRating.Create(ContentCategory.Sexual, 1),
                    CategoryRating.Create(ContentCategory.Profanity, 0),
                    CategoryRating.Create(ContentCategory.Alcohol, 0),
                    CategoryRating.Create(ContentCategory.Sensitive, 0)
                }
            ],
            [
                new[]
                {
                    CategoryRating.Create(ContentCategory.Violent, 0),
                    CategoryRating.Create(ContentCategory.Nudity, 0),
                    CategoryRating.Create(ContentCategory.Sexual, 0),
                    CategoryRating.Create(ContentCategory.Profanity, 1),
                    CategoryRating.Create(ContentCategory.Alcohol, 1),
                    CategoryRating.Create(ContentCategory.Sensitive, 0)
                }
            ],
            [
                new[]
                {
                    CategoryRating.Create(ContentCategory.Violent, 0),
                    CategoryRating.Create(ContentCategory.Nudity, 1),
                    CategoryRating.Create(ContentCategory.Sexual, 0),
                    CategoryRating.Create(ContentCategory.Profanity, 1),
                    CategoryRating.Create(ContentCategory.Alcohol, 1),
                    CategoryRating.Create(ContentCategory.Sensitive, 0)
                }
            ],
            [
                new[]
                {
                    CategoryRating.Create(ContentCategory.Violent, 1),
                    CategoryRating.Create(ContentCategory.Nudity, 2),
                    CategoryRating.Create(ContentCategory.Sexual, 1),
                    CategoryRating.Create(ContentCategory.Profanity, 0),
                    CategoryRating.Create(ContentCategory.Alcohol, 0),
                    CategoryRating.Create(ContentCategory.Sensitive, 1)
                }
            ]
        ];
    }

    [Theory]
    [MemberData(nameof(YoungAdultTestCases))]
    public void EvaluateContentShouldReturnYoungAdult(CategoryRating[] categoryRatings)
    {
        // Act
        string title = "TestSeries";
        string synopsis = "TestSeries";
        string? thumbnail = "TestSeries";
        SeriesType type = SeriesType.Comic;
        Genre[] genres = [new("TestGenre", "TestDes")];

        // Act
        Series series = Series.CreateNew(title, synopsis, thumbnail, type, genres, categoryRatings);

        // Assert
        series.Should().NotBeNull();
        series.ContentRating.Should().Be(ContentRating.YoungAdult);
    }

    public static IEnumerable<object[]> YoungAdultTestCases()
    {
        return
        [
            [
                new[]
                {
                    CategoryRating.Create(ContentCategory.Violent, 2),
                    CategoryRating.Create(ContentCategory.Nudity, 2),
                    CategoryRating.Create(ContentCategory.Sexual, 2),
                    CategoryRating.Create(ContentCategory.Profanity, 2),
                    CategoryRating.Create(ContentCategory.Alcohol, 2),
                    CategoryRating.Create(ContentCategory.Sensitive, 2)
                }
            ],
            [
                new[]
                {
                    CategoryRating.Create(ContentCategory.Violent, 2),
                    CategoryRating.Create(ContentCategory.Nudity, 1),
                    CategoryRating.Create(ContentCategory.Sexual, 1),
                    CategoryRating.Create(ContentCategory.Profanity, 2),
                    CategoryRating.Create(ContentCategory.Alcohol, 2),
                    CategoryRating.Create(ContentCategory.Sensitive, 1)
                }
            ],
            [
                new[]
                {
                    CategoryRating.Create(ContentCategory.Violent, 1),
                    CategoryRating.Create(ContentCategory.Nudity, 3),
                    CategoryRating.Create(ContentCategory.Sexual, 0),
                    CategoryRating.Create(ContentCategory.Profanity, 1),
                    CategoryRating.Create(ContentCategory.Alcohol, 2),
                    CategoryRating.Create(ContentCategory.Sensitive, 2)
                }
            ]
        ];
    }

    [Theory]
    [MemberData(nameof(MatureTestCases))]
    public void EvaluateContentShouldReturnMature(CategoryRating[] categoryRatings)
    {
        // Act
        string title = "TestSeries";
        string synopsis = "TestSeries";
        string? thumbnail = "TestSeries";
        SeriesType type = SeriesType.Comic;
        Genre[] genres = [new("TestGenre", "TestDes")];

        // Act
        Series series = Series.CreateNew(title, synopsis, thumbnail, type, genres, categoryRatings);

        // Assert
        series.Should().NotBeNull();
        series.ContentRating.Should().Be(ContentRating.Mature);
    }

    public static IEnumerable<object[]> MatureTestCases()
    {
        return
        [
            [
                new[]
                {
                    CategoryRating.Create(ContentCategory.Violent, 2),
                    CategoryRating.Create(ContentCategory.Nudity, 0),
                    CategoryRating.Create(ContentCategory.Sexual, 1),
                    CategoryRating.Create(ContentCategory.Profanity, 2),
                    CategoryRating.Create(ContentCategory.Alcohol, 1),
                    CategoryRating.Create(ContentCategory.Sensitive, 3)
                }
            ],
            [
                new[]
                {
                    CategoryRating.Create(ContentCategory.Violent, 3),
                    CategoryRating.Create(ContentCategory.Nudity, 3),
                    CategoryRating.Create(ContentCategory.Sexual, 3),
                    CategoryRating.Create(ContentCategory.Profanity, 3),
                    CategoryRating.Create(ContentCategory.Alcohol, 3),
                    CategoryRating.Create(ContentCategory.Sensitive, 3)
                }
            ]
        ];
    }

    [Theory]
    [MemberData(nameof(EvaluateContentShouldFailTestCases))]
    public void EvaluateContentShouldFail(CategoryRating[] categoryRatings)
    {
        // Act
        string title = "TestSeries";
        string synopsis = "TestSeries";
        string? thumbnail = "TestSeries";
        SeriesType type = SeriesType.Comic;
        Genre[] genres = [];

        // Act & Assert
        FluentActions.Invoking(() => Series.CreateNew(title, synopsis, thumbnail, type, genres, categoryRatings))
            .Should().Throw<ArgumentException>();
    }

    public static IEnumerable<object[]> EvaluateContentShouldFailTestCases()
    {
        return
        [
            [
                new CategoryRating[] { }
            ],
            [
                new[] { CategoryRating.Create(ContentCategory.Alcohol, 1) }
            ],
            [
                new[]
                {
                    CategoryRating.Create(ContentCategory.Violent, 1),
                    CategoryRating.Create(ContentCategory.Sexual, 0),
                    CategoryRating.Create(ContentCategory.Profanity, 2),
                    CategoryRating.Create(ContentCategory.Alcohol, 3)
                }
            ],
            [
                new[]
                {
                    CategoryRating.Create(ContentCategory.Violent, 1),
                    CategoryRating.Create(ContentCategory.Violent, 1),
                    CategoryRating.Create(ContentCategory.Violent, 1),
                    CategoryRating.Create(ContentCategory.Violent, 1),
                    CategoryRating.Create(ContentCategory.Violent, 1),
                    CategoryRating.Create(ContentCategory.Violent, 1)
                }
            ],
            [
                new[]
                {
                    CategoryRating.Create(ContentCategory.Nudity, 1),
                    CategoryRating.Create(ContentCategory.Violent, 1),
                    CategoryRating.Create(ContentCategory.Sensitive, 1),
                    CategoryRating.Create(ContentCategory.Violent, 1),
                    CategoryRating.Create(ContentCategory.Violent, 1),
                    CategoryRating.Create(ContentCategory.Sexual, 1),
                    CategoryRating.Create(ContentCategory.Violent, 1)
                }
            ]
        ];
    }

    [Fact]
    public void UpdateVolumeShouldUpdateSuccessfully()
    {
        // Arrange
        Series series = Series.CreateNew(
            "TestSeries",
            "TestSynopsis",
            "TestThumbnailUrl",
            SeriesType.Comic,
            [new Genre("TestGenre", "TestGenreDescription")],
            [
                CategoryRating.Create(ContentCategory.Violent, 1), CategoryRating.Create(ContentCategory.Nudity, 1),
                CategoryRating.Create(ContentCategory.Sexual, 0), CategoryRating.Create(ContentCategory.Profanity, 3),
                CategoryRating.Create(ContentCategory.Alcohol, 2), CategoryRating.Create(ContentCategory.Sensitive, 1)
            ]);

        // Act
        series.Update(
            "UpdatedTitle",
            "UpdatedSynopsis",
            "UpdatedThumbnail",
            SeriesStatus.Completed,
            [new Genre("Genre1", "Genre1")],
            [
                CategoryRating.Create(ContentCategory.Violent, 1), CategoryRating.Create(ContentCategory.Nudity, 1),
                CategoryRating.Create(ContentCategory.Sexual, 2), CategoryRating.Create(ContentCategory.Profanity, 2),
                CategoryRating.Create(ContentCategory.Alcohol, 2), CategoryRating.Create(ContentCategory.Sensitive, 1)
            ],
            ["Alt 1"],
            ["Author 1"],
            DateTimeOffset.UtcNow);

        // Assert
        series.Title.Should().Be("UpdatedTitle");
        series.Synopsis.Should().Be("UpdatedSynopsis");
        series.Thumbnail.Should().Be("UpdatedThumbnail");
        series.Status.Should().Be(SeriesStatus.Completed);
        series.Genres.Should().BeEquivalentTo([new Genre("Genre1", "Genre1")]);
        series.ContentRating.Should().Be(ContentRating.YoungAdult);
        series.AlternativeTitles.Should().BeEquivalentTo("Alt 1");
        series.Authors.Should().BeEquivalentTo("Author 1");
        series.StartTime.Should().BeWithin(TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public void AddNewVolumeShouldSuccess()
    {
        // Arrange
        Series series = Series.CreateNew(
            "TestSeries",
            "TestSynopsis",
            "TestThumbnailUrl",
            SeriesType.Comic,
            [new Genre("TestGenre", "TestGenreDescription")],
            [
                CategoryRating.Create(ContentCategory.Violent, 1), CategoryRating.Create(ContentCategory.Nudity, 1),
                CategoryRating.Create(ContentCategory.Sexual, 0), CategoryRating.Create(ContentCategory.Profanity, 3),
                CategoryRating.Create(ContentCategory.Alcohol, 2), CategoryRating.Create(ContentCategory.Sensitive, 1)
            ]);

        Volume volume = Volume.Create("TestName", "TestIntroduction");

        // Act
        series.AddNewVolume(volume);

        // Assert
        series.Volumes.Count.Should().Be(1);
        series.Volumes[0].Name.Should().Be("TestName");
        series.Volumes[0].Introduction.Should().Be("TestIntroduction");
        series.Volumes[0].VolumeNumber.Should().Be(1);
    }

    [Fact]
    public void AddMultipleVolumeShouldSuccess()
    {
        // Arrange
        Series series = Series.CreateNew(
            "TestSeries",
            "TestSynopsis",
            "TestThumbnailUrl",
            SeriesType.Comic,
            [new Genre("TestGenre", "TestGenreDescription")],
            [
                CategoryRating.Create(ContentCategory.Violent, 1), CategoryRating.Create(ContentCategory.Nudity, 1),
                CategoryRating.Create(ContentCategory.Sexual, 0), CategoryRating.Create(ContentCategory.Profanity, 3),
                CategoryRating.Create(ContentCategory.Alcohol, 2), CategoryRating.Create(ContentCategory.Sensitive, 1)
            ]);

        Volume volume1 = Volume.Create("TestName1", "TestIntroduction1");
        Volume volume2 = Volume.Create("TestName2", "TestIntroduction2");

        // Act
        series.AddNewVolume(volume1);
        series.AddNewVolume(volume2);

        // Assert
        series.Volumes.Count.Should().Be(2);
        series.Volumes[0].Name.Should().Be("TestName1");
        series.Volumes[0].Introduction.Should().Be("TestIntroduction1");
        series.Volumes[0].VolumeNumber.Should().Be(1);
        series.Volumes[1].Name.Should().Be("TestName2");
        series.Volumes[1].Introduction.Should().Be("TestIntroduction2");
        series.Volumes[1].VolumeNumber.Should().Be(2);
    }

    [Fact]
    public void DeleteVolumeShouldRemoveFromSeries()
    {
        // Arrange
        Series series = Series.CreateNew(
            "TestSeries",
            "TestSynopsis",
            "TestThumbnailUrl",
            SeriesType.Comic,
            [new Genre("TestGenre", "TestGenreDescription")],
            [
                CategoryRating.Create(ContentCategory.Violent, 1), CategoryRating.Create(ContentCategory.Nudity, 1),
                CategoryRating.Create(ContentCategory.Sexual, 0), CategoryRating.Create(ContentCategory.Profanity, 3),
                CategoryRating.Create(ContentCategory.Alcohol, 2), CategoryRating.Create(ContentCategory.Sensitive, 1)
            ]);

        Volume volume1 = Volume.Create("TestName1", "TestIntroduction1");
        volume1.Id = 1;

        Volume volume2 = Volume.Create("TestName2", "TestIntroduction2");
        volume2.Id = 2;

        Volume volume3 = Volume.Create("TestName3", "TestIntroduction3");
        volume3.Id = 3;

        series.AddNewVolume(volume1);
        series.AddNewVolume(volume2);
        series.AddNewVolume(volume3);

        // Act
        series.RemoveVolume(volume2.Id);

        // Assert
        series.VolumeCount.Should().Be(2);
        volume1.VolumeNumber.Should().Be(1);
        volume3.VolumeNumber.Should().Be(2);
    }

    [Fact]
    public void DeleteVolumeShouldRequireExistingVolume()
    {
        // Arrange
        Series series = Series.CreateNew(
            "TestSeries",
            "TestSynopsis",
            "TestThumbnailUrl",
            SeriesType.Comic,
            [new Genre("TestGenre", "TestGenreDescription")],
            [
                CategoryRating.Create(ContentCategory.Violent, 1), CategoryRating.Create(ContentCategory.Nudity, 1),
                CategoryRating.Create(ContentCategory.Sexual, 0), CategoryRating.Create(ContentCategory.Profanity, 3),
                CategoryRating.Create(ContentCategory.Alcohol, 2), CategoryRating.Create(ContentCategory.Sensitive, 1)
            ]);

        // Act
        FluentActions.Invoking(() => series.RemoveVolume(1)).Should().Throw<VolumeNotFoundException>();
    }

    [Fact]
    public void DeleteFirstVolumeShouldReorderChapterNumber()
    {
        // Arrange
        Series series = Series.CreateNew(
            "TestSeries",
            "TestSynopsis",
            "TestThumbnailUrl",
            SeriesType.Comic,
            [new Genre("TestGenre", "TestGenreDescription")],
            [
                CategoryRating.Create(ContentCategory.Violent, 1), CategoryRating.Create(ContentCategory.Nudity, 1),
                CategoryRating.Create(ContentCategory.Sexual, 0), CategoryRating.Create(ContentCategory.Profanity, 3),
                CategoryRating.Create(ContentCategory.Alcohol, 2), CategoryRating.Create(ContentCategory.Sensitive, 1)
            ]);

        Volume volume1 = Volume.Create("TestName1", "TestIntroduction1");
        volume1.Id = 1;

        Volume volume2 = Volume.Create("TestName2", "TestIntroduction2");
        volume2.Id = 2;

        Volume volume3 = Volume.Create("TestName3", "TestIntroduction3");
        volume3.Id = 3;

        series.AddNewVolume(volume1);
        series.AddNewVolume(volume2);
        series.AddNewVolume(volume3);

        // Act
        series.RemoveVolume(volume1.Id);

        // Assert
        series.VolumeCount.Should().Be(2);
        volume2.VolumeNumber.Should().Be(1);
        volume3.VolumeNumber.Should().Be(2);
    }

    [Fact]
    public void DeleteVolumeThenAddShouldHaveCorrectVolumeNumber()
    {
        // Arrange
        Series series = Series.CreateNew(
            "TestSeries",
            "TestSynopsis",
            "TestThumbnailUrl",
            SeriesType.Comic,
            [new Genre("TestGenre", "TestGenreDescription")],
            [
                CategoryRating.Create(ContentCategory.Violent, 1), CategoryRating.Create(ContentCategory.Nudity, 1),
                CategoryRating.Create(ContentCategory.Sexual, 0), CategoryRating.Create(ContentCategory.Profanity, 3),
                CategoryRating.Create(ContentCategory.Alcohol, 2), CategoryRating.Create(ContentCategory.Sensitive, 1)
            ]);

        Volume volume1 = Volume.Create("TestName1", "TestIntroduction1");
        volume1.Id = 1;

        Volume volume2 = Volume.Create("TestName2", "TestIntroduction2");
        volume2.Id = 2;

        Volume volume3 = Volume.Create("TestName3", "TestIntroduction3");
        volume3.Id = 3;

        series.AddNewVolume(volume1);
        series.AddNewVolume(volume2);

        // Act
        series.RemoveVolume(volume2.Id);
        series.AddNewVolume(volume3);

        // Assert
        series.VolumeCount.Should().Be(2);
        volume1.VolumeNumber.Should().Be(1);
        volume3.VolumeNumber.Should().Be(2);
    }

    [Fact]
    public void AddNewDashiFanTierShouldAddSuccessfully()
    {
        // Arrange
        Series series = Series.CreateNew(
            "TestSeries",
            "TestSynopsis",
            "TestThumbnailUrl",
            SeriesType.Comic,
            [new Genre("TestGenre", "TestGenreDescription")],
            [
                CategoryRating.Create(ContentCategory.Violent, 1), CategoryRating.Create(ContentCategory.Nudity, 1),
                CategoryRating.Create(ContentCategory.Sexual, 0), CategoryRating.Create(ContentCategory.Profanity, 3),
                CategoryRating.Create(ContentCategory.Alcohol, 2), CategoryRating.Create(ContentCategory.Sensitive, 1)
            ]);

        DashiFan tier = DashiFan.Create(
            "Tier1",
            "DashiFan",
            4,
            10_000,
            "USD"
        );

        // Act
        series.AddNewDashiFan(tier);

        // Assert
        series.Tiers.Count.Should().Be(1);
        series.Tiers[0].Name.Should().Be("Tier1");
        series.Tiers[0].Description.Should().Be("DashiFan");
        series.Tiers[0].Perks.Should().Be(4);
        series.Tiers[0].IsActive.Should().Be(true);
        series.Tiers[0].Price.Amount.Should().Be(10_000);
        series.Tiers[0].Price.Currency.Should().Be("USD");
    }
}
