using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Exceptions;
using FluentAssertions;

namespace Domain.UnitTests;

public class VolumeTests
{
    [Fact]
    public void CreateVolumeShouldCreateSuccessfully()
    {
        // Arrange
        string name = "TestName";
        string introduction = "TestIntroduction";

        // Act
        Volume volume = Volume.Create(name, introduction);

        // Assert
        volume.Should().NotBeNull();
        volume.Name.Should().Be(name);
        volume.Introduction.Should().Be(introduction);
        volume.VolumeNumber.Should().Be(0);
        volume.Chapters.Should().HaveCount(0);
    }

    [Theory]
    [MemberData(nameof(CreateVolumeShouldRequireValidNameTestCases))]
    public void CreateVolumeShouldRequireValidName(string name)
    {
        // Arrange
        string introduction = "TestIntroduction";

        // Act
        FluentActions
            .Invoking(() => Volume.Create(name, introduction))
            .Should().Throw<ArgumentException>();
    }

    public static IEnumerable<object[]> CreateVolumeShouldRequireValidNameTestCases()
    {
        return
        [
            [
                string.Empty
            ],
            [
                new string('*', 101)
            ]
        ];
    }

    [Theory]
    [MemberData(nameof(CreateVolumeShouldRequireValidIntroductionTestCases))]
    public void CreateVolumeShouldRequireValidIntroduction(string introduction)
    {
        // Arrange
        string name = "TestName";

        // Act
        FluentActions
            .Invoking(() => Volume.Create(name, introduction))
            .Should().Throw<ArgumentException>();
    }

    public static IEnumerable<object[]> CreateVolumeShouldRequireValidIntroductionTestCases()
    {
        return
        [
            [
                new string('*', 2001)
            ]
        ];
    }

    [Fact]
    public void UpdateVolumeShouldUpdateSuccessfully()
    {
        // Arrange
        Volume volume = Volume.Create("TestName", "TestIntroduction");

        string newName = "NewName";
        string newIntroduction = "NewIntroduction";

        // Act
        volume.Update(newName, newIntroduction);

        // Assert
        volume.Should().NotBeNull();
        volume.Name.Should().Be(newName);
        volume.Introduction.Should().Be(newIntroduction);
    }

    [Theory]
    [MemberData(nameof(UpdateVolumeShouldRequireValidNameTestCases))]
    public void UpdateVolumeShouldRequireValidName(string name)
    {
        // Arrange
        Volume volume = Volume.Create("TestName", "TestIntroduction");

        // Act
        FluentActions
            .Invoking(() => volume.Update(name, "TestIntroduction"))
            .Should().Throw<ArgumentException>();
    }

    public static IEnumerable<object[]> UpdateVolumeShouldRequireValidNameTestCases()
    {
        return
        [
            [
                string.Empty
            ],
            [
                new string('*', 101)
            ]
        ];
    }

    [Theory]
    [MemberData(nameof(UpdateVolumeShouldRequireValidIntroductionTestCases))]
    public void UpdateVolumeShouldRequireValidIntroduction(string introduction)
    {
        // Arrange
        Volume volume = Volume.Create("TestName", "TestIntroduction");

        // Act
        FluentActions
            .Invoking(() => volume.Update("TestName", introduction))
            .Should().Throw<ArgumentException>();
    }

    public static IEnumerable<object[]> UpdateVolumeShouldRequireValidIntroductionTestCases()
    {
        return
        [
            [
                new string('*', 2001)
            ]
        ];
    }

    [Fact]
    public void AddNewChapterShouldSuccess()
    {
        // Arrange
        Volume volume = Volume.Create("Test", "Test");

        Chapter chapter = Chapter.Create("Title", "Thumbnail", "Content", "Note");

        // Act
        volume.AddNewChapter(chapter);

        // Assert
        volume.ChapterCount.Should().Be(1);
        volume.Chapters.Should().HaveCount(1);
        chapter.ChapterNumber.Should().Be(1);
    }

    [Fact]
    public void AddMultipleChapterShouldSuccess()
    {
        // Arrange
        Volume volume = Volume.Create("Test", "Test");

        Chapter chapter1 = Chapter.Create("Title", "Thumbnail", "Content", "Note");
        Chapter chapter2 = Chapter.Create("Title", "Thumbnail", "Content", "Note");
        Chapter chapter3 = Chapter.Create("Title", "Thumbnail", "Content", "Note");

        // Act
        volume.AddNewChapter(chapter1);
        volume.AddNewChapter(chapter2);
        volume.AddNewChapter(chapter3);

        // Assert
        volume.ChapterCount.Should().Be(3);
        volume.Chapters.Should().HaveCount(3);

        chapter1.ChapterNumber.Should().Be(1);
        chapter2.ChapterNumber.Should().Be(2);
        chapter3.ChapterNumber.Should().Be(3);
    }

    [Fact]
    public void RemoveChapterShouldSuccess()
    {
        // Arrange
        Volume volume = Volume.Create("Test", "Test");

        Chapter chapter = Chapter.Create("Title", "Thumbnail", "Content", "Note");
        chapter.Id = 1;

        volume.AddNewChapter(chapter);

        // Act
        volume.RemoveChapter(1);

        // Assert
        volume.Should().NotBeNull();
        volume.Chapters.Should().HaveCount(0);
        volume.ChapterCount.Should().Be(0);
    }

    [Fact]
    public void RemoveChapterShouldReorderChapters()
    {
        // Arrange
        Volume volume = Volume.Create("Test", "Test");

        Chapter chapter1 = Chapter.Create("Title", "Thumbnail", "Content", "Note");
        chapter1.Id = 1;

        Chapter chapter2 = Chapter.Create("Title", "Thumbnail", "Content", "Note");
        chapter2.Id = 2;

        Chapter chapter3 = Chapter.Create("Title", "Thumbnail", "Content", "Note");
        chapter3.Id = 3;

        volume.AddNewChapter(chapter1);
        volume.AddNewChapter(chapter2);
        volume.AddNewChapter(chapter3);

        // Act
        volume.RemoveChapter(chapter1.Id);

        // Assert
        volume.ChapterCount.Should().Be(2);
        volume.Chapters.Should().HaveCount(2);
        volume.Chapters.Should().NotContain(chapter1);

        chapter2.ChapterNumber.Should().Be(1);
        chapter3.ChapterNumber.Should().Be(2);
    }

    [Fact]
    public void RemoveChapterThenAddNewChapterShouldHaveCorrectChapterNumber()
    {
        // Arrange
        Volume volume = Volume.Create("Test", "Test");

        Chapter chapter1 = Chapter.Create("Title", "Thumbnail", "Content", "Note");
        chapter1.Id = 1;

        Chapter chapter2 = Chapter.Create("Title", "Thumbnail", "Content", "Note");
        chapter2.Id = 2;

        Chapter chapter3 = Chapter.Create("Title", "Thumbnail", "Content", "Note");
        chapter3.Id = 3;

        Chapter chapter4 = Chapter.Create("Title", "Thumbnail", "Content", "Note");
        chapter4.Id = 4;

        volume.AddNewChapter(chapter1);
        volume.AddNewChapter(chapter2);
        volume.AddNewChapter(chapter3);

        // Act
        volume.RemoveChapter(chapter1.Id);

        volume.AddNewChapter(chapter4);

        // Assert
        volume.Chapters.Should().HaveCount(3);
        volume.ChapterCount.Should().Be(3);

        chapter2.ChapterNumber.Should().Be(1);
        chapter3.ChapterNumber.Should().Be(2);
        chapter4.ChapterNumber.Should().Be(3);
    }

    [Fact]
    public void RemoveNonExistingChapterShouldFail()
    {
        // Arrange
        Volume volume = Volume.Create("Test", "Test");

        Chapter chapter1 = Chapter.Create("Title", "Thumbnail", "Content", "Note");
        chapter1.Id = 1;

        Chapter chapter2 = Chapter.Create("Title", "Thumbnail", "Content", "Note");
        chapter2.Id = 2;

        Chapter chapter3 = Chapter.Create("Title", "Thumbnail", "Content", "Note");
        chapter3.Id = 3;

        volume.AddNewChapter(chapter1);
        volume.AddNewChapter(chapter2);
        volume.AddNewChapter(chapter3);

        // Act & Assert
        FluentActions.Invoking(() => volume.RemoveChapter(4)).Should().Throw<ChapterNotFoundException>();
    }

    [Fact]
    public void ReorderChapterBackToFrontShouldHaveCorrectOrder()
    {
        // Arrange
        Volume volume = Volume.Create("Test", "Test");

        Chapter chapter1 = Chapter.Create("Title1", "Thumbnail1", "Content1", "Note1");
        chapter1.Id = 1;

        Chapter chapter2 = Chapter.Create("Title2", "Thumbnail2", "Content2", "Note2");
        chapter2.Id = 2;

        Chapter chapter3 = Chapter.Create("Title3", "Thumbnail3", "Content3", "Note3");
        chapter3.Id = 3;

        Chapter chapter4 = Chapter.Create("Title4", "Thumbnail4", "Content4", "Note4");
        chapter4.Id = 4;

        volume.AddNewChapter(chapter1);
        volume.AddNewChapter(chapter2);
        volume.AddNewChapter(chapter3);
        volume.AddNewChapter(chapter4);

        // Act
        volume.ReorderChapter(chapter3.Id, chapter1.Id);

        // Assert
        chapter1.ChapterNumber.Should().Be(1);
        chapter2.ChapterNumber.Should().Be(3);
        chapter4.ChapterNumber.Should().Be(4);

        chapter3.ChapterNumber.Should().Be(2);
    }

    [Fact]
    public void ReorderChapterFrontToBackShouldHaveCorrectOrder()
    {
        // Arrange
        Volume volume = Volume.Create("Test", "Test");

        Chapter chapter1 = Chapter.Create("Title1", "Thumbnail1", "Content1", "Note1");
        chapter1.Id = 1;

        Chapter chapter2 = Chapter.Create("Title2", "Thumbnail2", "Content2", "Note2");
        chapter2.Id = 2;

        Chapter chapter3 = Chapter.Create("Title3", "Thumbnail3", "Content3", "Note3");
        chapter3.Id = 3;

        Chapter chapter4 = Chapter.Create("Title4", "Thumbnail4", "Content4", "Note4");
        chapter4.Id = 4;

        volume.AddNewChapter(chapter1);
        volume.AddNewChapter(chapter2);
        volume.AddNewChapter(chapter3);
        volume.AddNewChapter(chapter4);

        // Act
        volume.ReorderChapter(chapter1.Id, chapter3.Id);

        // Assert
        chapter2.ChapterNumber.Should().Be(1);
        chapter3.ChapterNumber.Should().Be(2);
        chapter1.ChapterNumber.Should().Be(3);
        chapter4.ChapterNumber.Should().Be(4);
    }

    [Fact]
    public void ReorderChapterToFirstChapterShouldHaveCorrectOrder()
    {
        // Arrange
        Volume volume = Volume.Create("Test", "Test");

        Chapter chapter1 = Chapter.Create("Title1", "Thumbnail1", "Content1", "Note1");
        chapter1.Id = 1;

        Chapter chapter2 = Chapter.Create("Title2", "Thumbnail2", "Content2", "Note2");
        chapter2.Id = 2;

        Chapter chapter3 = Chapter.Create("Title3", "Thumbnail3", "Content3", "Note3");
        chapter3.Id = 3;

        Chapter chapter4 = Chapter.Create("Title4", "Thumbnail4", "Content4", "Note4");
        chapter4.Id = 4;

        volume.AddNewChapter(chapter1);
        volume.AddNewChapter(chapter2);
        volume.AddNewChapter(chapter3);
        volume.AddNewChapter(chapter4);

        // Act
        volume.ReorderChapter(chapter3.Id, 0);

        // Assert
        chapter3.ChapterNumber.Should().Be(1);

        chapter1.ChapterNumber.Should().Be(2);
        chapter2.ChapterNumber.Should().Be(3);
        chapter4.ChapterNumber.Should().Be(4);
    }

    [Fact]
    public void ReorderChapterToLastChapterShouldHaveCorrectOrder()
    {
        // Arrange
        Volume volume = Volume.Create("Test", "Test");

        Chapter chapter1 = Chapter.Create("Title1", "Thumbnail1", "Content1", "Note1");
        chapter1.Id = 1;

        Chapter chapter2 = Chapter.Create("Title2", "Thumbnail2", "Content2", "Note2");
        chapter2.Id = 2;

        Chapter chapter3 = Chapter.Create("Title3", "Thumbnail3", "Content3", "Note3");
        chapter3.Id = 3;

        Chapter chapter4 = Chapter.Create("Title4", "Thumbnail4", "Content4", "Note4");
        chapter4.Id = 4;

        volume.AddNewChapter(chapter1);
        volume.AddNewChapter(chapter2);
        volume.AddNewChapter(chapter3);
        volume.AddNewChapter(chapter4);

        // Act
        volume.ReorderChapter(chapter2.Id, chapter4.Id);

        // Assert
        chapter1.ChapterNumber.Should().Be(1);
        chapter2.ChapterNumber.Should().Be(4);
        chapter3.ChapterNumber.Should().Be(2);
        chapter4.ChapterNumber.Should().Be(3);
    }

    [Fact]
    public void ReorderChapterShouldRequireExistingTargetChapter()
    {
        // Arrange
        Volume volume = Volume.Create("Test", "Test");

        // Act
        FluentActions.Invoking(() => volume.ReorderChapter(1, 2))
            .Should().Throw<ChapterNotFoundException>();
    }

    [Fact]
    public void ReorderChapterShouldRequireExistingPreviousChapter()
    {
        // Arrange
        Volume volume = Volume.Create("Test", "Test");

        Chapter chapter1 = Chapter.Create("Title1", "Thumbnail1", "Content1", "Note1");
        chapter1.Id = 1;

        Chapter chapter2 = Chapter.Create("Title2", "Thumbnail2", "Content2", "Note2");
        chapter2.Id = 2;

        Chapter chapter3 = Chapter.Create("Title3", "Thumbnail3", "Content3", "Note3");
        chapter3.Id = 3;

        volume.AddNewChapter(chapter1);
        volume.AddNewChapter(chapter2);
        volume.AddNewChapter(chapter3);

        // Act
        FluentActions.Invoking(() => volume.ReorderChapter(2, 4))
            .Should().Throw<ChapterNotFoundException>();
    }
}
