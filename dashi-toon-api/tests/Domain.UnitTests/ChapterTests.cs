using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using DashiToon.Api.Domain.Exceptions;
using FluentAssertions;

namespace Domain.UnitTests;

public class ChapterTests
{
    [Fact]
    public void CreateChapterShouldCreateSuccessfully()
    {
        // Arrange
        string title = "Title";
        string thumbnail = "Thumbnail";
        string content = "Content";
        string note = "Note";

        // Act
        Chapter chapter = Chapter.Create(title, thumbnail, content, note);

        // Assert
        chapter.Should().NotBeNull();
        chapter.GetCurrentVersion().VersionName.Should().Contain("Bản Thảo");
        chapter.GetCurrentVersion().Title.Should().Be(title);
        chapter.GetCurrentVersion().Thumbnail.Should().Be(thumbnail);
        chapter.GetCurrentVersion().Content.Should().Be(content);
        chapter.GetCurrentVersion().Note.Should().Be(note);
        chapter.GetCurrentVersion().Status.Should().Be(ChapterStatus.Draft);
        chapter.PublishedDate.Should().BeNull();
        chapter.PublishedVersionId.Should().BeNull();
        chapter.IsAdvanceChapter.Should().BeFalse();
        chapter.KanaPrice.Should().BeNull();
        chapter.Versions.Should().HaveCount(1);
    }

    [Fact]
    public void UpdateChapterShouldUpdateSuccessfully()
    {
        // Arrange
        string title = "Title";
        string thumbnail = "Thumbnail";
        string content = "Content";
        string note = "Note";

        Chapter chapter = Chapter.Create(title, thumbnail, content, note);

        Guid versionId = chapter.CurrentVersionId;

        // Act
        chapter.Update("newTitle", "newThumbnail", "newContent", "newNote");

        // Assert
        chapter.Should().NotBeNull();
        chapter.GetCurrentVersion().VersionName.Should().Contain("Bản Thảo");
        chapter.GetCurrentVersion().Title.Should().Be("newTitle");
        chapter.GetCurrentVersion().Thumbnail.Should().Be("newThumbnail");
        chapter.GetCurrentVersion().Content.Should().Be("newContent");
        chapter.GetCurrentVersion().Note.Should().Be("newNote");
        chapter.GetCurrentVersion().Status.Should().Be(ChapterStatus.Draft);
        chapter.PublishedDate.Should().BeNull();
        chapter.PublishedVersionId.Should().BeNull();
        chapter.IsAdvanceChapter.Should().BeFalse();
        chapter.KanaPrice.Should().BeNull();
        chapter.Versions.Should().HaveCount(2);
        chapter.CurrentVersionId.Should().NotBe(versionId);
    }

    [Fact]
    public void SaveChapterShouldSaveSuccessfully()
    {
        // Arrange
        string title = "Title";
        string thumbnail = "Thumbnail";
        string content = "Content";
        string note = "Note";

        Chapter chapter = Chapter.Create(title, thumbnail, content, note);

        Guid versionId = chapter.CurrentVersionId;

        // Act
        chapter.Save("newTitle", "newThumbnail", "newContent", "newNote");

        // Assert
        chapter.Should().NotBeNull();
        chapter.GetCurrentVersion().VersionName.Should().Contain("Bản Lưu");
        chapter.GetCurrentVersion().Title.Should().Be("newTitle");
        chapter.GetCurrentVersion().Thumbnail.Should().Be("newThumbnail");
        chapter.GetCurrentVersion().Content.Should().Be("newContent");
        chapter.GetCurrentVersion().Note.Should().Be("newNote");
        chapter.GetCurrentVersion().Status.Should().Be(ChapterStatus.Draft);
        chapter.PublishedDate.Should().BeNull();
        chapter.PublishedVersionId.Should().BeNull();
        chapter.IsAdvanceChapter.Should().BeFalse();
        chapter.KanaPrice.Should().BeNull();
        chapter.Versions.Should().HaveCount(2);
        chapter.CurrentVersionId.Should().NotBe(versionId);
    }

    [Fact]
    public void PublishImmediatelyShouldChangeStatus()
    {
        // Arrange
        string title = "Title";
        string thumbnail = "Thumbnail";
        string content = "Content";
        string note = "Note";

        Chapter chapter = Chapter.Create(title, thumbnail, content, note);

        // Act
        chapter.PublishImmediately();

        // Assert
        chapter.GetCurrentVersion().Status.Should().Be(ChapterStatus.Published);
        chapter.GetPublishedVersion().Should().NotBeNull();
        chapter.PublishedDate.Should().NotBeNull();
        chapter.PublishedDate!.Value.Should().BeWithin(TimeSpan.FromMilliseconds(100));
        chapter.PublishedVersionId.Should().Be(chapter.GetCurrentVersion().Id);
        chapter.IsAdvanceChapter.Should().BeFalse();
    }

    [Fact]
    public void SchedulePublishShouldChangeStatus()
    {
        // Arrange
        string title = "Title";
        string thumbnail = "Thumbnail";
        string content = "Content";
        string note = "Note";

        Chapter chapter = Chapter.Create(title, thumbnail, content, note);

        DateTimeOffset scheduledDate = DateTimeOffset.UtcNow + TimeSpan.FromDays(1);
        // Act
        chapter.SchedulePublish(scheduledDate);

        // Assert
        chapter.GetCurrentVersion().Status.Should().Be(ChapterStatus.Published);
        chapter.GetPublishedVersion().Should().NotBeNull();

        chapter.IsAdvanceChapter.Should().BeTrue();
        chapter.PublishedDate.Should().Be(scheduledDate);
        chapter.PublishedVersionId.Should().Be(chapter.GetCurrentVersion().Id);
    }

    [Fact]
    public void SchedulePublishShouldRequireValidScheduledDate()
    {
        // Arrange
        string title = "Title";
        string thumbnail = "Thumbnail";
        string content = "Content";
        string note = "Note";

        Chapter chapter = Chapter.Create(title, thumbnail, content, note);

        DateTimeOffset scheduledDate = DateTimeOffset.UtcNow;
        // Act
        FluentActions.Invoking(() => chapter.SchedulePublish(scheduledDate))
            .Should().Throw<InvalidPublishDateException>();
    }

    [Fact]
    public void PublishPublishedChapterShouldFail()
    {
        // Arrange
        string title = "Title";
        string thumbnail = "Thumbnail";
        string content = "Content";
        string note = "Note";

        Chapter chapter = Chapter.Create(title, thumbnail, content, note);

        chapter.PublishImmediately();

        // Act && Assert
        FluentActions.Invoking(() => chapter.PublishImmediately())
            .Should().Throw<PublishMoreThanOnceException>();
    }

    [Fact]
    public void UnpublishShouldChangeStatus()
    {
        // Arrange
        string title = "Title";
        string thumbnail = "Thumbnail";
        string content = "Content";
        string note = "Note";

        Chapter chapter = Chapter.Create(title, thumbnail, content, note);
        chapter.PublishImmediately();

        // Act
        chapter.Unpublish();

        // Assert
        chapter.GetCurrentVersion().Status.Should().Be(ChapterStatus.Draft);
        chapter.PublishedVersionId.Should().BeNull();
        chapter.PublishedDate.Should().BeNull();
    }

    [Fact]
    public void UnpublishNonPublishedChapterShouldFail()
    {
        // Arrange
        string title = "Title";
        string thumbnail = "Thumbnail";
        string content = "Content";
        string note = "Note";

        Chapter chapter = Chapter.Create(title, thumbnail, content, note);

        // Act & Assert
        FluentActions.Invoking(() => chapter.Unpublish())
            .Should().Throw<UnpublishNonPublishedChapterException>();
    }

    [Fact]
    public void RestoreChapterShouldChangeCurrentVersion()
    {
        // Arrange
        string title = "Title";
        string thumbnail = "Thumbnail";
        string content = "Content";
        string note = "Note";
        Chapter chapter = Chapter.Create(title, thumbnail, content, note);

        Guid originalVersionId = chapter.CurrentVersionId;

        chapter.Update("NewTitle", "NewThumbnail", "NewContent", "NewNote");

        // Act
        chapter.RestoreVersion(originalVersionId);

        // Assert
        chapter.Should().NotBeNull();
        chapter.GetCurrentVersion().VersionName.Should().Contain("Bản Thảo");
        chapter.GetCurrentVersion().Title.Should().Be(title);
        chapter.GetCurrentVersion().Thumbnail.Should().Be(thumbnail);
        chapter.GetCurrentVersion().Content.Should().Be(content);
        chapter.GetCurrentVersion().Note.Should().Be(note);
        chapter.GetCurrentVersion().Status.Should().Be(ChapterStatus.Draft);
        chapter.PublishedDate.Should().BeNull();
        chapter.Versions.Should().HaveCount(2);
    }

    [Fact]
    public void RestoreChapterShouldRequireExistingVersion()
    {
        // Arrange
        string title = "Title";
        string thumbnail = "Thumbnail";
        string content = "Content";
        string note = "Note";
        Chapter chapter = Chapter.Create(title, thumbnail, content, note);

        // Act & Assert
        FluentActions.Invoking(() => chapter.RestoreVersion(Guid.NewGuid()))
            .Should().Throw<ChapterVersionNotFoundException>();
    }

    [Fact]
    public void UpdateChapterVersionShouldSuccess()
    {
        // Arrange
        string title = "Title";
        string thumbnail = "Thumbnail";
        string content = "Content";
        string note = "Note";
        Chapter chapter = Chapter.Create(title, thumbnail, content, note);

        // Act
        chapter.UpdateVersion(chapter.CurrentVersionId, "What do you want to do?");

        // Assert
        chapter.GetCurrentVersion().VersionName.Should().Be("What do you want to do?");
    }

    [Fact]
    public void UpdateChapterVersionShouldRequireExistingVersion()
    {
        // Arrange
        string title = "Title";
        string thumbnail = "Thumbnail";
        string content = "Content";
        string note = "Note";
        Chapter chapter = Chapter.Create(title, thumbnail, content, note);

        // Act
        FluentActions.Invoking(() => chapter.UpdateVersion(Guid.NewGuid(), "What do you want to do?"))
            .Should().Throw<ChapterVersionNotFoundException>();
    }

    [Fact]
    public void RemoveChapterVersionShouldSuccess()
    {
        // Arrange
        string title = "Title";
        string thumbnail = "Thumbnail";
        string content = "Content";
        string note = "Note";
        Chapter chapter = Chapter.Create(title, thumbnail, content, note);

        Guid originalVersionId = chapter.CurrentVersionId;

        chapter.Update("Title1", "Thumbnail1", "Content1", "Note1");

        // Act
        chapter.RemoveVersion(originalVersionId);

        // Assert
        chapter.Versions.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveChapterVersionShouldRequireExistingVersion()
    {
        // Arrange
        string title = "Title";
        string thumbnail = "Thumbnail";
        string content = "Content";
        string note = "Note";
        Chapter chapter = Chapter.Create(title, thumbnail, content, note);

        // Act & Assert
        FluentActions.Invoking(() => chapter.RemoveVersion(Guid.NewGuid()))
            .Should().Throw<ChapterVersionNotFoundException>();
    }

    [Fact]
    public void RemoveChapterVersionShouldNotBeAllowedForCurrentVersion()
    {
        // Arrange
        string title = "Title";
        string thumbnail = "Thumbnail";
        string content = "Content";
        string note = "Note";
        Chapter chapter = Chapter.Create(title, thumbnail, content, note);

        // Act & Assert
        FluentActions.Invoking(() => chapter.RemoveVersion(chapter.CurrentVersionId))
            .Should().Throw<DeleteInUseVersionChapterException>();
    }

    [Fact]
    public void RemoveChapterVersionShouldNotBeAllowedForPublishedVersion()
    {
        // Arrange
        string title = "Title";
        string thumbnail = "Thumbnail";
        string content = "Content";
        string note = "Note";
        Chapter chapter = Chapter.Create(title, thumbnail, content, note);

        Guid originalVersionId = chapter.CurrentVersionId;

        chapter.PublishImmediately();

        chapter.Update("Title1", "Thumbnail1", "Content1", "Note1");

        // Act & Assert
        FluentActions.Invoking(() => chapter.RemoveVersion(originalVersionId))
            .Should().Throw<DeleteInUseVersionChapterException>();
    }

    [Fact]
    public void SetChapterPricingShouldSuccess()
    {
        // Arrange
        string title = "Title";
        string thumbnail = "Thumbnail";
        string content = "Content";
        string note = "Note";
        Chapter chapter = Chapter.Create(title, thumbnail, content, note);

        // Act
        chapter.SetPrice(60);

        // Assert
        chapter.KanaPrice.Should().Be(60);
    }

    [Fact]
    public void SetChapterPricingShouldRequireValidPrice()
    {
        // Arrange
        string title = "Title";
        string thumbnail = "Thumbnail";
        string content = "Content";
        string note = "Note";
        Chapter chapter = Chapter.Create(title, thumbnail, content, note);

        // Act
        FluentActions.Invoking(() => chapter.SetPrice(0))
            .Should()
            .Throw<ArgumentException>();
    }

    [Fact]
    public void AddReadingAnalyticShouldIncreaseViewCount()
    {
        // Arrange
        string title = "Title";
        string thumbnail = "Thumbnail";
        string content = "Content";
        string note = "Note";
        Chapter chapter = Chapter.Create(title, thumbnail, content, note);

        // Act
        chapter.AddReadingAnalytic(10);

        // Assert

        chapter.ViewCount.Should().Be(10);
        chapter.Analytics.Should().HaveCount(1);
        chapter.Analytics.ElementAt(0).ViewCount.Should().Be(10);
        chapter.Analytics.ElementAt(0).Timestamp.Should().BeWithin(TimeSpan.FromMilliseconds(100));
    }
}
