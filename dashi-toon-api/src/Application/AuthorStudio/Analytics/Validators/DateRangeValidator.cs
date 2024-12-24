using DashiToon.Api.Application.AuthorStudio.Analytics.Models;

namespace DashiToon.Api.Application.AuthorStudio.Analytics.Validators;

public class DateRangeValidator : AbstractValidator<DateRange>
{
    public DateRangeValidator()
    {
        RuleFor(dr => dr.To)
            .NotEmpty();

        RuleFor(dr => dr.From)
            .NotEmpty();

        RuleFor(dr => dr.To)
            .GreaterThanOrEqualTo(dr => dr.From)
            .WithMessage("From must be greater than or equal To");
    }
}
