using DashiToon.Api.Application.AuthorStudio.Analytics.Models;

namespace DashiToon.Api.Application.AuthorStudio.Analytics.Validators;

public class BaseAnalyticsQueryValidator : AbstractValidator<BaseAnalyticsQuery>
{
    public BaseAnalyticsQueryValidator()
    {
        RuleFor(x => x.Current)
            .SetValidator(new DateRangeValidator());

        RuleFor(x => x.Compare)
            .SetValidator(new DateRangeValidator()!)
            .When(x => x.Compare is not null);
    }
}
