using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using FluentAssertions;

namespace Domain.UnitTests;

public class CategoryRatingTests
{
    [Fact]
    public void CreateCategoryRatingShouldCreateSuccessfully()
    {
        // Act 
        CategoryRating categoryRating = CategoryRating.Create(ContentCategory.Nudity, 1);

        categoryRating.Should().NotBeNull();

        categoryRating.Category.Should().Be(ContentCategory.Nudity);
        categoryRating.Rating.Should().Be(1);
    }

    [Fact]
    public void CreateCategoryRatingShouldRequireValidRating()
    {
        // Act & Assert
        FluentActions.Invoking(() => CategoryRating.Create(ContentCategory.Nudity, -1))
            .Should().Throw<ArgumentException>();
    }
}
