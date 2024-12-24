using System.Data.Common;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bogus;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Dapper;
using DashiToon.Api.Application.Administrator.Series.ExportSeries;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using DashiToon.Api.Domain.ValueObjects;
using DashiToon.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace DashiToon.Api.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();

        ApplicationDbContextInitialiser initialiser = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();

        await initialiser.SeedAsync(initialiser.TrySeedMasterDataAsync);
    }

    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();

        ApplicationDbContextInitialiser initialiser = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.SeedAsync(initialiser.TrySeedDataAsync);
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public ApplicationDbContextInitialiser(
        ILogger<ApplicationDbContextInitialiser> logger,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync(Func<Task> seedData)
    {
        try
        {
            await seedData();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedMasterDataAsync()
    {
        // Default roles
        IdentityRole administratorRole = new(Roles.Administrator);

        if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
        {
            await _roleManager.CreateAsync(administratorRole);
        }

        IdentityRole moderatorRole = new(Roles.Moderator);

        if (_roleManager.Roles.All(r => r.Name != moderatorRole.Name))
        {
            await _roleManager.CreateAsync(moderatorRole);
        }

        // Default users
        ApplicationUser administrator = new()
        {
            UserName = "Admin", Email = "administrator@dashitoon.shutano.com", EmailConfirmed = true
        };

        if (_userManager.Users.All(u => u.UserName != administrator.UserName))
        {
            await _userManager.CreateAsync(administrator, "Administrator1!");
            if (!string.IsNullOrWhiteSpace(administratorRole.Name))
            {
                await _userManager.AddToRolesAsync(administrator, [administratorRole.Name]);
            }
        }

        ApplicationUser moderator = new()
        {
            UserName = "Moderator", Email = "moderator@dashitoon.shutano.com", EmailConfirmed = true
        };

        if (_userManager.Users.All(u => u.UserName != moderator.UserName))
        {
            await _userManager.CreateAsync(moderator, "Moderator1!");
            if (!string.IsNullOrWhiteSpace(moderatorRole.Name))
            {
                await _userManager.AddToRolesAsync(moderator, [moderatorRole.Name]);
            }
        }

        // Seed Data
        if (!_context.KanaGoldPacks.Any())
        {
            _context.KanaGoldPacks.AddRange(
                KanaGoldPack.Create(3400, Price.CreateNew(34_000M, "VND"), true),
                KanaGoldPack.Create(4200, Price.CreateNew(42_000M, "VND"), true),
                KanaGoldPack.Create(6900, Price.CreateNew(69_000M, "VND"), true),
                KanaGoldPack.Create(7100, Price.CreateNew(71_000M, "VND"), true),
                KanaGoldPack.Create(9900, Price.CreateNew(99_000M, "VND"), true));
            await _context.SaveChangesAsync();
        }

        if (!_context.Missions.Any())
        {
            _context.Missions.AddRange(
                Mission.CreateNew(3, 50, true),
                Mission.CreateNew(4, 50, true),
                Mission.CreateNew(6, 50, true),
                Mission.CreateNew(9, 50, true));
            await _context.SaveChangesAsync();
        }

        if (!_context.KanaExchangeRates.Any())
        {
            _context.KanaExchangeRates.Add(KanaExchangeRate.Create(
                "VND",
                10
            ));
            await _context.SaveChangesAsync();
        }

        if (!_context.CommissionRates.Any())
        {
            _context.CommissionRates.Add(CommissionRate.Create(
                CommissionType.Kana,
                30
            ));

            _context.CommissionRates.Add(CommissionRate.Create(
                CommissionType.DashiFan,
                30
            ));
            await _context.SaveChangesAsync();
        }
    }

    public async Task TrySeedDataAsync()
    {
        string? jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "ExportedSeries.json");

        using FileStream fileStream = File.OpenRead(jsonFilePath);

        List<ExportSeriesVm>? importedSeries =
            await JsonSerializer.DeserializeAsync<List<ExportSeriesVm>>(fileStream,
                new JsonSerializerOptions { WriteIndented = true });

        Dictionary<string, Genre>? genreTranslationMap = new()
        {
            {
                "action", new Genre(
                    "Hành động",
                    "Thể loại tập trung vào các pha hành động, chiến đấu căng thẳng và kịch tính.")
            },
            {
                "adventure", new Genre(
                    "Phiêu lưu",
                    "Thể loại kể về những cuộc hành trình và khám phá đầy thú vị.")
            },
            {
                "harem", new Genre(
                    "Hậu cung",
                    "Thể loại xoay quanh một nhân vật nam hoặc nữ được nhiều người yêu mến.")
            },
            {
                "martial arts", new Genre(
                    "Võ thuật",
                    "Thể loại tập trung vào các kỹ thuật chiến đấu và võ thuật.")
            },
            {
                "mature", new Genre(
                    "Trưởng thành",
                    "Thể loại dành cho người trưởng thành với nội dung phức tạp và sâu sắc.")
            },
            {
                "xuanhuan", new Genre(
                    "Huyền huyễn",
                    "Thể loại giả tưởng có yếu tố siêu nhiên và phép thuật.")
            },
            {
                "mystery", new Genre(
                    "Bí ẩn",
                    "Thể loại xoay quanh việc khám phá các bí ẩn và vụ án.")
            },
            {
                "xianxia", new Genre(
                    "Tiên hiệp",
                    "Thể loại tập trung vào tu tiên và các yếu tố thần thoại Trung Quốc.")
            },
            {
                "drama", new Genre(
                    "Kịch tính",
                    "Thể loại tập trung vào sự phát triển tâm lý nhân vật và các tình huống căng thẳng.")
            },
            {
                "fantasy", new Genre(
                    "Giả tưởng",
                    "Thể loại có các yếu tố phép thuật, sinh vật huyền thoại và thế giới tưởng tượng.")
            },
            {
                "seinen", new Genre(
                    "Seinen",
                    "Thể loại hướng đến đối tượng nam giới trưởng thành với nội dung sâu sắc.")
            },
            {
                "mecha", new Genre(
                    "Cơ giáp",
                    "Thể loại tập trung vào các robot khổng lồ và công nghệ tiên tiến.")
            },
            {
                "romance", new Genre(
                    "Lãng mạn",
                    "Thể loại xoay quanh các câu chuyện tình yêu và cảm xúc lãng mạn.")
            },
            {
                "sci-fi", new Genre(
                    "Khoa học viễn tưởng",
                    "Thể loại giả tưởng với yếu tố công nghệ và khoa học tiên tiến.")
            },
            {
                "shounen", new Genre(
                    "Shounen",
                    "Thể loại hướng đến đối tượng thiếu niên nam với các câu chuyện hành động và phiêu lưu.")
            },
            {
                "slice of life", new Genre(
                    "Đời thường",
                    "Thể loại phản ánh cuộc sống hàng ngày và các mối quan hệ xã hội.")
            },
            {
                "ecchi", new Genre(
                    "Ecchi",
                    "Thể loại có yếu tố hài hước và gợi cảm nhưng không quá mức.")
            },
            {
                "comedy", new Genre(
                    "Hài hước",
                    "Thể loại tập trung vào các yếu tố hài hước, gây cười.")
            },
            {
                "historical", new Genre(
                    "Lịch sử",
                    "Thể loại lấy bối cảnh lịch sử hoặc liên quan đến các sự kiện trong quá khứ.")
            },
            {
                "tragedy", new Genre(
                    "Bi kịch",
                    "Thể loại có nội dung đau thương, cảm động và thường kết thúc buồn.")
            },
            {
                "school life", new Genre(
                    "Học đường",
                    "Thể loại xoay quanh cuộc sống của học sinh trong trường học.")
            },
            {
                "smut", new Genre(
                    "Smut",
                    "Thể loại có yếu tố tình dục rõ rệt và nhạy cảm.")
            },
            {
                "yaoi", new Genre(
                    "Yaoi",
                    "Thể loại tập trung vào mối quan hệ tình cảm giữa nam giới.")
            },
            {
                "supernatural", new Genre(
                    "Siêu nhiên",
                    "Thể loại có các yếu tố siêu nhiên như ma quỷ, linh hồn và phép thuật.")
            },
            {
                "wuxia", new Genre(
                    "Võ hiệp",
                    "Thể loại võ thuật cổ trang Trung Quốc với các anh hùng võ hiệp.")
            },
            {
                "psychological", new Genre(
                    "Tâm lý",
                    "Thể loại tập trung vào các yếu tố tâm lý và suy nghĩ của nhân vật.")
            },
            {
                "shoujo", new Genre(
                    "Shoujo",
                    "Thể loại hướng đến đối tượng nữ thiếu niên, với các câu chuyện lãng mạn.")
            },
            {
                "josei", new Genre(
                    "Josei",
                    "Thể loại dành cho phụ nữ trưởng thành, tập trung vào các mối quan hệ và đời sống.")
            },
            {
                "adult", new Genre(
                    "Người lớn",
                    "Thể loại dành cho người lớn với các chủ đề trưởng thành và nhạy cảm.")
            },
            {
                "horror", new Genre(
                    "Kinh dị",
                    "Thể loại tập trung vào yếu tố kinh dị, gây sợ hãi và căng thẳng.")
            },
            {
                "shounen ai", new Genre(
                    "Shounen Ai",
                    "Thể loại tập trung vào tình cảm nhẹ nhàng giữa nam giới.")
            },
            {
                "gender bender", new Genre(
                    "Hoán đổi giới tính",
                    "Thể loại có nhân vật hoán đổi giới tính hoặc biến đổi cơ thể.")
            },
            {
                "shoujo ai", new Genre(
                    "Shoujo Ai",
                    "Thể loại tập trung vào mối quan hệ tình cảm giữa nữ giới.")
            },
            {
                "yuri", new Genre(
                    "Yuri",
                    "Thể loại xoay quanh tình yêu giữa nữ giới.")
            },
            {
                "sports", new Genre(
                    "Thể thao",
                    "Thể loại tập trung vào các môn thể thao và sự cạnh tranh trong thi đấu.")
            }
        };

        if (!_context.Genres.Any())
        {
            _context.Genres.AddRange(genreTranslationMap.Values);

            await _context.SaveChangesAsync();
        }

        if (!_context.Series.Any())
        {
            ApplicationUser? admin = await _userManager.FindByNameAsync("Admin");

            Dictionary<string, Genre> genresMap = _context.Genres.ToDictionary(g => g.Name, g => g);

            List<Series> series = (importedSeries ?? []).Select(record =>
                {
                    Series? series = Series.CreateNew(
                            record.Title,
                            record.Synopsis,
                            record.Thumbnail,
                            record.Type,
                            record.Genres.Select(genre => genresMap[genre.Trim()]).ToArray(),
                            record.CategoryRatings.Select(cr => CategoryRating.Create(cr.Category, cr.Rating))
                                .ToArray(),
                            record.AlternativeTitles,
                            record.Authors,
                            record.StartTime
                        )
                        .WithImmediatePublish();

                    series.WithSeedVolumes(record.Volumes.Select(v =>
                    {
                        return Volume.Create(
                            v.Name,
                            v.Introduction
                        ).WithSeedChapter(v.Chapters.Select(c => Chapter.Create(
                                c.Title,
                                c.Thumbnail,
                                c.Content,
                                c.Note
                            )).ToArray(),
                            v.VolumeNumber);
                    }).ToArray());

                    return series;
                }
            ).ToList();

            _context.Series.AddRange(series);

            await _context.SaveChangesAsync();

            DbConnection? connection = _context.Database.GetDbConnection();

            await connection.ExecuteAsync(
                """
                UPDATE "Series" SET "CreatedBy" = @CreatedBy;
                """,
                new { CreatedBy = admin?.Id ?? string.Empty });
        }

        var faker = new Faker();

        for (int i = 0; i < 20; i++)
        {
            var firstName = faker.Name.FirstName();

            var newUser = new ApplicationUser
            {
                UserName = firstName, Email = faker.Internet.Email(firstName), EmailConfirmed = true
            };

            await _userManager.CreateAsync(newUser, "DitLangMan@123");
        }
    }
}

public class StringArrayConverter : DefaultTypeConverter
{
    public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<string>(); // or string[] if you're using an array
        }

        // Trim and remove the square brackets, then split the elements
        string? cleanedText = text.Trim('[', ']', ' ');
        string[]? tags = cleanedText.Split(',')
            .Select(t => t.Trim('\'', ' ')) // Remove quotes and whitespace
            .ToArray();

        return tags; // return tags.ToArray() for string[] type
    }
}

public sealed class NovelRecordMap : ClassMap<NovelRecord>
{
    public NovelRecordMap()
    {
        Map(m => m.Name).Name("name");
        Map(m => m.AssociatedNames).Name("assoc_names").TypeConverter<StringArrayConverter>();
        Map(m => m.Authors).Name("authors").TypeConverter<StringArrayConverter>();
        Map(m => m.StartYear).Name("start_year");
        Map(m => m.Genres).Name("genres").TypeConverter<StringArrayConverter>();
    }
}

public class NovelRecord
{
    public string Name { get; set; } = null!;
    public string[] AssociatedNames { get; set; } = null!;
    public string[] Authors { get; set; } = null!;
    public int StartYear { get; set; }
    public string[] Genres { get; set; } = null!;
}

internal static class SeriesBuilder
{
    private static Faker _faker = new();
    private static Random _random = new();

    internal static Series WithImmediatePublish(this Series series)
    {
        series.Update(
            series.Title,
            series.Synopsis,
            series.Thumbnail,
            SeriesStatus.Ongoing,
            series.Genres.ToArray(),
            series.CategoryRatings.ToArray(),
            series.AlternativeTitles.ToArray(),
            series.Authors.ToArray(),
            series.StartTime);

        series.ClearDomainEvents();

        return series;
    }

    internal static Series WithSeedVolumes(this Series series)
    {
        for (int i = 0; i < 2; i++)
        {
            Volume? volume = Volume.Create(
                _faker.Lorem.Sentence(3, 6),
                _faker.Lorem.Sentence(6));

            volume.ClearDomainEvents();

            series.AddNewVolume(volume);
        }

        return series;
    }

    internal static Series WithSeedVolumes(this Series series, Volume[] volumes)
    {
        foreach (Volume volume in volumes)
        {
            volume.ClearDomainEvents();

            series.AddNewVolume(volume);
        }

        return series;
    }

    internal static Volume WithSeedChapter(this Volume volume)
    {
        StringBuilder? builder = new();

        SeriesType seriesType = volume.Series.Type;

        for (int i = 0; i < 3; i++)
        {
            int chapterPrice;

            if (seriesType == SeriesType.Novel)
            {
                int wordCount = _random.Next(40, 50);
                int lineCount = _random.Next(50, 60);

                for (int j = 0; j < lineCount; j++)
                {
                    builder.Append($"<p>{_faker.Lorem.Sentence(wordCount)}</p>");
                }

                chapterPrice = wordCount * lineCount / 100;
            }
            else
            {
                int imageCount = _random.Next(10, 20);

                builder.Append('[');

                builder.Append("\"default.png\"");

                for (int j = 1; j < imageCount; j++)
                {
                    builder.Append(",\"default.png\"");
                }

                builder.Append(']');

                chapterPrice = imageCount * 2;
            }

            Chapter? chapter = Chapter.Create(
                _faker.Lorem.Sentence(3, 10),
                null,
                builder.ToString(),
                null
            );

            chapter.PublishImmediately();

            if (volume.VolumeNumber != 1)
            {
                chapter.SetPrice(chapterPrice);
            }

            chapter.ClearDomainEvents();
            volume.AddNewChapter(chapter);
            volume.ClearDomainEvents();

            builder.Clear();
        }

        return volume;
    }

    internal static Volume WithSeedChapter(this Volume volume, Chapter[] chapters, int volumeNumber)
    {
        foreach (Chapter? chapter in chapters)
        {
            chapter.PublishImmediately();

            if (volumeNumber != 1)
            {
                chapter.SetPrice(_faker.Random.Int(35, 50));
            }

            chapter.ClearDomainEvents();
            volume.AddNewChapter(chapter);
            volume.ClearDomainEvents();
        }

        return volume;
    }
}

public class SeedNovel
{
    public required string Thumbnail { get; set; }
    public required string Title { get; set; }
    public required string Synopsis { get; set; }
    public required string ContentRating { get; set; }
    public List<string> Authors { get; set; } = [];
    public List<string> Genres { get; set; } = [];
    public List<SeedVolume> Volumes { get; set; } = [];
    public DateTimeOffset PublishedDate { get; set; }
}

public class SeedVolume
{
    public int VolumeNumber { get; set; }
    public List<SeedChapter> Chapters { get; set; } = [];
}

public class SeedChapter
{
    public int ChapterNumber { get; set; }
    public required string ChapterThumbnail { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
}
