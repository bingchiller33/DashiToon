namespace DashiToon.Api.Application.Images.Queries.GetImageUrl;

public class GetImageUrlQueryValidator : AbstractValidator<GetImageUrlQuery>
{
    public GetImageUrlQueryValidator()
    {
        RuleFor(x => x.Type)
            .Must(BeValidType)
            .WithMessage("Type must be thumbnails or chapters");
    }

    private bool BeValidType(string type)
    {
        return type is "thumbnails" or "chapters";
    }
}
