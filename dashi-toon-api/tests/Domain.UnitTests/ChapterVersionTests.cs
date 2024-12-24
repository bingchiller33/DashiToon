using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using FluentAssertions;

namespace Domain.UnitTests;

public class ChapterVersionTests
{
    public static IEnumerable<object[]> CreateChapterVersionShouldRequireValidTitleTestCases =>
    [
        [
            new string('*', 256)
        ],
        [
            string.Empty
        ]
    ];

    public static IEnumerable<object[]> CreateChapterVersionShouldRequireValidNoteTestCases =>
    [
        [
            new string('*', 2001)
        ]
    ];

    [Fact]
    public void CreateAutoSaveVersionShouldBeSuccessful()
    {
        // Arrange
        string title = "Title";
        string thumbnail = "Thumbnail";
        string content = "Content";
        string note = "Note";

        // Act
        ChapterVersion version = ChapterVersion.Create(title, thumbnail, content, note, true);

        // Assert
        version.VersionName.Should().Contain("Bản Lưu");
        version.Title.Should().Be(title);
        version.Thumbnail.Should().Be(thumbnail);
        version.Content.Should().Be(content);
        version.Note.Should().Be(note);
        version.Status.Should().Be(ChapterStatus.Draft);
        version.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(100));
        version.IsAutoSave.Should().BeTrue();
    }

    [Fact]
    public void CreateSaveVersionShouldBeSuccessful()
    {
        // Arrange
        string title = "Title";
        string thumbnail = "Thumbnail";
        string content = "Content";
        string note = "Note";

        // Act
        ChapterVersion version = ChapterVersion.Create(title, thumbnail, content, note, true);

        // Assert
        version.VersionName.Should().Contain("Bản Lưu");
        version.Title.Should().Be(title);
        version.Thumbnail.Should().Be(thumbnail);
        version.Content.Should().Be(content);
        version.Note.Should().Be(note);
        version.Status.Should().Be(ChapterStatus.Draft);
        version.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMilliseconds(100));
        version.IsAutoSave.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(CreateChapterVersionShouldRequireValidTitleTestCases))]
    public void CreateChapterVersionShouldRequireValidTitle(string title)
    {
        // Arrange
        string thumbnail = "Thumbnail";
        string content = "Content";
        string note = "Note";

        // Act
        FluentActions.Invoking(() => ChapterVersion.Create(title, thumbnail, content, note, false))
            .Should().Throw<ArgumentException>();
    }

    [Theory]
    [MemberData(nameof(CreateChapterVersionShouldRequireValidNoteTestCases))]
    public void CreateChapterVersionShouldRequireValidNote(string note)
    {
        // Arrange
        string title = "Title";
        string thumbnail = "Thumbnail";
        string content = "Content";

        // Act
        FluentActions.Invoking(() => ChapterVersion.Create(title, thumbnail, content, note, false))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateChapterVersionShouldBeSuccessful()
    {
        // Arrange
        string title = "Title";
        string thumbnail = "Thumbnail";
        string content = "Content";
        string note = "Note";
        ChapterVersion version = ChapterVersion.Create(title, thumbnail, content, note, true);

        // Act
        version.ChangeName("I LOVE YOU");

        // Assert
        version.VersionName.Should().Be("I LOVE YOU");
    }
}
