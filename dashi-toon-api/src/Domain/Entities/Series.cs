using System.Collections.Immutable;

namespace DashiToon.Api.Domain.Entities;

public class Series : BaseAuditableEntity<int>
{
    private readonly List<CategoryRating> _categoryRatings = [];

    private readonly List<Genre> _genres = [];
    private readonly List<Review> _reviews = [];
    private readonly List<DashiFan> _tiers = [];
    private readonly List<Volume> _volumes = [];

    private Series()
    {
    }

    public string Title { get; private set; } = null!;
    public string Synopsis { get; private set; } = null!;
    public string? Thumbnail { get; private set; }
    public SeriesType Type { get; private set; }
    public SeriesStatus Status { get; private set; }
    public ContentRating ContentRating { get; private set; }
    public string[] AlternativeTitles { get; private set; } = [];
    public string[] Authors { get; private set; } = [];
    public int VolumeCount { get; private set; }
    public DateTimeOffset? StartTime { get; private set; }

    public IReadOnlyList<Genre> Genres => _genres.AsReadOnly();
    public IReadOnlyList<Volume> Volumes => _volumes.OrderBy(v => v.VolumeNumber).ToImmutableList();
    public IReadOnlyList<Review> Reviews => _reviews.AsReadOnly();
    public IReadOnlyList<CategoryRating> CategoryRatings => _categoryRatings.AsReadOnly();
    public IReadOnlyList<DashiFan> Tiers => _tiers.AsReadOnly();

    public static Series CreateNew(
        string title,
        string synopsis,
        string? thumbnail,
        SeriesType type,
        Genre[] genres,
        CategoryRating[] categoryRatings,
        string[]? alternativeTitles = null,
        string[]? authors = null,
        DateTimeOffset? startTime = null)
    {
        ValidateTitle(title);

        ValidateSynopsis(synopsis);

        ValidateGenres(genres);

        ContentRating contentRating = EvaluateContent(categoryRatings);

        Series series = new()
        {
            Title = title,
            Synopsis = synopsis,
            Thumbnail = thumbnail,
            Type = type,
            Status = SeriesStatus.Draft,
            ContentRating = contentRating,
            AlternativeTitles = alternativeTitles ?? [],
            Authors = authors ?? [],
            StartTime = startTime
        };

        series._genres.AddRange(genres);
        series._categoryRatings.AddRange(categoryRatings);

        series.AddDomainEvent(new SeriesCreatedEvent(series));

        return series;
    }

    public void Update(
        string newTitle,
        string newSynopsis,
        string? newThumbnail,
        SeriesStatus newStatus,
        Genre[] newGenres,
        CategoryRating[] newCategoryRatings,
        string[]? newAlternativeTitles = null,
        string[]? newAuthors = null,
        DateTimeOffset? newStartTime = null)
    {
        ValidateTitle(newTitle);
        Title = newTitle;

        ValidateSynopsis(newSynopsis);
        Synopsis = newSynopsis;

        Thumbnail = newThumbnail;

        StartTime = newStartTime;

        if (newStatus == SeriesStatus.Ongoing)
        {
            Status = newStatus;

            StartTime ??= DateTimeOffset.UtcNow;
        }
        else
        {
            Status = newStatus;
        }

        AlternativeTitles = newAlternativeTitles ?? [];
        Authors = newAuthors ?? [];

        ValidateGenres(newGenres);
        _genres.Clear();
        _genres.AddRange(newGenres);

        ContentRating = EvaluateContent(newCategoryRatings);
        _categoryRatings.Clear();
        _categoryRatings.AddRange(newCategoryRatings);

        AddDomainEvent(new SeriesUpdatedEvent(this));
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrEmpty(title))
        {
            throw new ArgumentNullException(nameof(title));
        }

        if (title.Length is < 1 or > 255)
        {
            throw new ArgumentException("Title must be between 1 and 255 characters long.", nameof(title));
        }
    }

    private static void ValidateSynopsis(string synopsis)
    {
        if (string.IsNullOrEmpty(synopsis))
        {
            throw new ArgumentNullException(nameof(synopsis));
        }

        if (synopsis.Length is < 1 or > 5000)
        {
            throw new ArgumentException("Synopsis must be between 1 and 5000 characters long.", nameof(synopsis));
        }
    }

    private static void ValidateGenres(Genre[] genres)
    {
        if (genres.Length is < 1)
        {
            throw new ArgumentException("A series must have at least 1 genre", nameof(genres));
        }
    }

    private static ContentRating EvaluateContent(CategoryRating[] categoryRatings)
    {
        if (categoryRatings.Length != 6 || categoryRatings.Select(x => x.Category).Distinct().Count() != 6)
        {
            throw new ArgumentException("Rating can must have length of 6 and have no duplicate category");
        }


        Dictionary<(ContentCategory category, int option), ContentRating> contentCategoryMap =
            new()
            {
                { (ContentCategory.Violent, 1), ContentRating.Teen },
                { (ContentCategory.Violent, 2), ContentRating.YoungAdult },
                { (ContentCategory.Violent, 3), ContentRating.Mature },
                { (ContentCategory.Nudity, 1), ContentRating.Teen },
                { (ContentCategory.Nudity, 2), ContentRating.Teen },
                { (ContentCategory.Nudity, 3), ContentRating.YoungAdult },
                { (ContentCategory.Sexual, 1), ContentRating.Teen },
                { (ContentCategory.Sexual, 2), ContentRating.YoungAdult },
                { (ContentCategory.Sexual, 3), ContentRating.Mature },
                { (ContentCategory.Profanity, 1), ContentRating.Teen },
                { (ContentCategory.Profanity, 2), ContentRating.YoungAdult },
                { (ContentCategory.Profanity, 3), ContentRating.Mature },
                { (ContentCategory.Alcohol, 1), ContentRating.Teen },
                { (ContentCategory.Alcohol, 2), ContentRating.YoungAdult },
                { (ContentCategory.Alcohol, 3), ContentRating.Mature },
                { (ContentCategory.Sensitive, 1), ContentRating.Teen },
                { (ContentCategory.Sensitive, 2), ContentRating.YoungAdult },
                { (ContentCategory.Sensitive, 3), ContentRating.Mature }
            };

        ContentRating result = ContentRating.AllAges;

        foreach (CategoryRating categoryRating in categoryRatings)
        {
            if (contentCategoryMap.TryGetValue((categoryRating.Category, categoryRating.Rating),
                    out ContentRating rating))
            {
                if (rating > result)
                {
                    result = rating;
                }
            }
        }

        return result;
    }

    public void AddNewVolume(Volume volume)
    {
        volume.VolumeNumber = ++VolumeCount;

        _volumes.Add(volume);
    }

    public void RemoveVolume(int volumeId)
    {
        Volume? targetVolume = _volumes.FirstOrDefault(v => v.Id == volumeId);

        if (targetVolume is null)
        {
            throw new VolumeNotFoundException();
        }

        foreach (Volume volume in _volumes.Where(v => v.VolumeNumber > targetVolume.VolumeNumber))
        {
            volume.VolumeNumber--;
        }

        _volumes.Remove(targetVolume);
        VolumeCount--;
    }

    public void AddNewDashiFan(DashiFan tier)
    {
        _tiers.Add(tier);
    }

    public void SuspendSeries()
    {
        Status = SeriesStatus.Hiatus;
    }
}

public class CategoryRating
{
    private CategoryRating()
    {
    }

    public ContentCategory Category { get; private set; }
    public int Rating { get; private set; }

    public static CategoryRating Create(ContentCategory category, int rating)
    {
        if (rating is < 0 or > 3)
        {
            throw new ArgumentException("Rating can only be from 0 to 3", nameof(rating));
        }

        return new CategoryRating { Category = category, Rating = rating };
    }
}
