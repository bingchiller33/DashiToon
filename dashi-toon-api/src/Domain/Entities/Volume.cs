namespace DashiToon.Api.Domain.Entities;

public class Volume : BaseEntity<int>
{
    private readonly List<Chapter> _chapters = [];

    private Volume()
    {
    }

    public string Name { get; private set; } = null!;
    public string? Introduction { get; private set; }
    public int VolumeNumber { get; internal set; }
    public int ChapterCount { get; private set; }
    public int SeriesId { get; }
    public Series Series { get; private set; } = null!;
    public IReadOnlyList<Chapter> Chapters => _chapters.AsReadOnly();


    public static Volume Create(string name, string? introduction)
    {
        ValidateName(name);

        ValidateIntroduction(introduction);

        Volume volume = new() { Name = name, Introduction = introduction };

        return volume;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrEmpty(name) || name.Length is < 0 or > 100)
        {
            throw new ArgumentException("Volume name must be between 0 and 100 characters.", nameof(name));
        }
    }

    private static void ValidateIntroduction(string? introduction)
    {
        if (!string.IsNullOrEmpty(introduction) && introduction.Length > 2000)
        {
            throw new ArgumentException("Introduction have a max length of 2000 characters.", nameof(introduction));
        }
    }

    public void Update(string name, string? introduction)
    {
        ValidateName(name);
        Name = name;

        ValidateIntroduction(introduction);
        Introduction = introduction;
    }

    public void AddNewChapter(Chapter chapter)
    {
        chapter.ChapterNumber = ++ChapterCount;

        _chapters.Add(chapter);
    }

    public void RemoveChapter(int chapterId)
    {
        Chapter? targetChapter = _chapters.FirstOrDefault(c => c.Id == chapterId);

        if (targetChapter is null)
        {
            throw new ChapterNotFoundException();
        }

        foreach (Chapter chapter in _chapters.Where(c => c.ChapterNumber > targetChapter.ChapterNumber))
        {
            chapter.ChapterNumber--;
        }

        _chapters.Remove(targetChapter);
        ChapterCount--;
    }

    public void ReorderChapter(int chapterId, int previousChapterId)
    {
        List<Chapter> chapters = _chapters.OrderBy(c => c.ChapterNumber).ToList();

        Chapter? targetChapter = chapters.FirstOrDefault(c => c.Id == chapterId);

        if (targetChapter is null)
        {
            throw new ChapterNotFoundException();
        }

        foreach (Chapter chapter in chapters.Where(c => c.ChapterNumber > targetChapter.ChapterNumber))
        {
            chapter.ChapterNumber--;
        }

        if (previousChapterId == 0) // prevId = 0 means reorder to first chapter
        {
            foreach (Chapter chapter in chapters)
            {
                chapter.ChapterNumber++;
            }

            targetChapter.ChapterNumber = 1;
        }
        else
        {
            Chapter? previousChapter = chapters.FirstOrDefault(c => c.Id == previousChapterId);

            if (previousChapter is null)
            {
                throw new ChapterNotFoundException();
            }

            foreach (Chapter chapter in chapters.Where(c => c.ChapterNumber > previousChapter.ChapterNumber))
            {
                chapter.ChapterNumber++;
            }

            targetChapter.ChapterNumber = previousChapter.ChapterNumber + 1;
        }
    }

    // public void PublishChapter(int chapterId)
    // {
    //     var chapter = _chapters.FirstOrDefault(c => c.Id == chapterId);
    //
    //     if (chapter is null)
    //     {
    //         throw new ChapterNotExistException(chapterId);
    //     }
    //
    //     var previousChapter = _chapters.FirstOrDefault(c => c.ChapterNumber == chapter.ChapterNumber - 1);
    //
    //     if (previousChapter is null)
    //     {
    //         throw new Exception("Chapter doesn't have previous chapter.");
    //     }
    //
    //     if (previousChapter.PublishedDate is null || previousChapter.PublishedDate > DateTimeOffset.UtcNow)
    //     {
    //         throw new PublishNotInOrderException();
    //     }
    //
    //     chapter.PublishImmediately();
    // }
    //
    // public void SchedulePublishChapter(int chapterId, DateTimeOffset publishedDate)
    // {
    //     var currentTime = DateTimeOffset.UtcNow;
    //
    //     var chapter = _chapters.FirstOrDefault(c => c.Id == chapterId);
    //
    //     if (chapter is null)
    //     {
    //         throw new ChapterNotExistException(chapterId);
    //     }
    //
    //     if (publishedDate <= currentTime)
    //     {
    //         throw new InvalidPublishDateException();
    //     }
    //
    //     var previousChapter = _chapters.FirstOrDefault(c => c.ChapterNumber == chapter.ChapterNumber - 1);
    //
    //     if (previousChapter is null)
    //     {
    //         throw new Exception("Chapter doesn't have previous chapter.");
    //     }
    //
    //     if (previousChapter.PublishedDate is null)
    //     {
    //         throw new PublishNotInOrderException();
    //     }
    //
    //     if (previousChapter.PublishedDate <= currentTime)
    //     {
    //         chapter.SchedulePublish(publishedDate);
    //     }
    // }
}
