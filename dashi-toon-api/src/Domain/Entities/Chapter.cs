namespace DashiToon.Api.Domain.Entities;

public class Chapter : BaseAuditableEntity<int>
{
    private readonly List<Comment> _comments = [];

    private readonly List<ChapterVersion> _versions = [];
    private readonly List<ReadingAnalytic> _analytics = [];

    private Chapter()
    {
    }

    public int ChapterNumber { get; internal set; }
    public Guid CurrentVersionId { get; private set; }
    public Guid? PublishedVersionId { get; private set; }
    public DateTimeOffset? PublishedDate { get; private set; }

    public bool IsAdvanceChapter => PublishedVersionId.HasValue
                                    && PublishedDate.HasValue
                                    && PublishedDate.Value > DateTimeOffset.UtcNow;

    public int ViewCount { get; private set; }
    public int? KanaPrice { get; private set; }
    public int VolumeId { get; }
    public Volume Volume { get; private set; } = null!;
    public IReadOnlyList<Comment> Comments => _comments.AsReadOnly();
    public IReadOnlyList<ChapterVersion> Versions => _versions.AsReadOnly();
    public IReadOnlyList<ReadingAnalytic> Analytics => _analytics.AsReadOnly();

    public static Chapter Create(
        string title,
        string? thumbnail,
        string content,
        string? note)
    {
        Chapter chapter = new();

        chapter.NewVersion(title, thumbnail, content, note, false);

        return chapter;
    }

    public void Update(
        string title,
        string? thumbnail,
        string content,
        string? note)
    {
        NewVersion(title, thumbnail, content, note, false);
    }

    public void Save(
        string title,
        string? thumbnail,
        string content,
        string? note)
    {
        NewVersion(title, thumbnail, content, note);
    }

    private void NewVersion(
        string title,
        string? thumbnail,
        string content,
        string? note,
        bool isAutoSave = true)
    {
        ChapterVersion version = ChapterVersion.Create(title, thumbnail, content, note, isAutoSave);

        CurrentVersionId = version.Id;
        _versions.Add(version);
    }

    public void SetPrice(int? price)
    {
        if (price is <= 0)
        {
            throw new ArgumentException("Price cannot be less than 0");
        }

        KanaPrice = price;
    }

    public void PublishImmediately()
    {
        GetCurrentVersion().Publish();

        PublishedVersionId = CurrentVersionId;
        PublishedDate = DateTimeOffset.UtcNow;

        AddDomainEvent(new ChapterPublishedEvent(this));
    }

    public void SchedulePublish(DateTimeOffset publishedDate)
    {
        if (publishedDate < DateTimeOffset.UtcNow)
        {
            throw new InvalidPublishDateException();
        }

        GetCurrentVersion().Publish();

        PublishedVersionId = CurrentVersionId;
        PublishedDate = publishedDate;

        AddDomainEvent(new ChapterPublishedEvent(this));
    }

    public void Unpublish()
    {
        ChapterVersion? publishedVersion = GetPublishedVersion();

        if (publishedVersion is null)
        {
            throw new UnpublishNonPublishedChapterException();
        }

        publishedVersion.Unpublish();

        PublishedVersionId = null;
        PublishedDate = null;
    }

    public void RestoreVersion(Guid versionId)
    {
        ChapterVersion? version = _versions.FirstOrDefault(v => v.Id == versionId);

        if (version is null)
        {
            throw new ChapterVersionNotFoundException();
        }

        CurrentVersionId = version.Id;
    }

    public void UpdateVersion(Guid versionId, string versionName)
    {
        ChapterVersion? version = _versions.FirstOrDefault(v => v.Id == versionId);

        if (version is null)
        {
            throw new ChapterVersionNotFoundException();
        }

        version.ChangeName(versionName);
    }

    public void RemoveVersion(Guid versionId)
    {
        ChapterVersion? version = _versions.FirstOrDefault(v => v.Id == versionId);

        if (version is null)
        {
            throw new ChapterVersionNotFoundException();
        }

        if (version.Id == CurrentVersionId || version.Id == PublishedVersionId)
        {
            throw new DeleteInUseVersionChapterException();
        }

        _versions.Remove(version);
    }

    public ChapterVersion GetCurrentVersion()
    {
        return _versions.Find(v => v.Id == CurrentVersionId)!;
    }

    public ChapterVersion? GetPublishedVersion()
    {
        return _versions.Find(v => v.Id == PublishedVersionId);
    }

    public void AddReadingAnalytic(int viewCount)
    {
        _analytics.Add(new ReadingAnalytic(viewCount));

        ViewCount = _analytics.Sum(a => a.ViewCount);
    }
}

public class ChapterVersion
{
    private ChapterVersion()
    {
    }

    public Guid Id { get; private set; }
    public bool IsAutoSave { get; private set; }
    public string VersionName { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string? Thumbnail { get; private set; }
    public string Content { get; private set; } = null!;
    public string? Note { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }
    public ChapterStatus Status { get; private set; }

    public static ChapterVersion Create(
        string title,
        string? thumbnail,
        string content,
        string? note,
        bool isAutoSave)
    {
        ValidateTitle(title);

        ValidateNote(note);

        Guid versionId = Guid.NewGuid();

        DateTimeOffset timeStamp = DateTimeOffset.UtcNow;

        string prefixVersion = isAutoSave ? "Bản Lưu" : "Bản Thảo";

        string versionName = $"{prefixVersion} @ {timeStamp.ToLocalTime():g}";
        return new ChapterVersion
        {
            Id = versionId,
            IsAutoSave = isAutoSave,
            VersionName = versionName,
            Title = title,
            Thumbnail = thumbnail,
            Content = content,
            Note = note,
            Timestamp = timeStamp,
            Status = ChapterStatus.Draft
        };
    }

    public void ChangeName(string versionName)
    {
        VersionName = versionName;
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrEmpty(title) || title.Length is < 1 or > 255)
        {
            throw new ArgumentException("Title can only have a length between 1 and 255 characters", nameof(title));
        }
    }

    private static void ValidateNote(string? note)
    {
        if (!string.IsNullOrEmpty(note) && note.Length > 2000)
        {
            throw new ArgumentException("Note can only have a max length of 2000 characters", nameof(note));
        }
    }

    internal void Publish()
    {
        if (Status == ChapterStatus.Published)
        {
            throw new PublishMoreThanOnceException();
        }

        Status = ChapterStatus.Published;
    }

    internal void Unpublish()
    {
        Status = ChapterStatus.Draft;
    }
}

public class ReadingAnalytic(int viewCount)
{
    public int ViewCount { get; init; } = viewCount;
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
