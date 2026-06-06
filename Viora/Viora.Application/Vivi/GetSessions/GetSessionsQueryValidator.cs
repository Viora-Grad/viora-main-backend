using FluentValidation;

namespace Viora.Application.Vivi.GetSessions;

internal class GetSessionsQueryValidator : AbstractValidator<GetSessionsQuery>
{
    public GetSessionsQueryValidator()
    {
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page must be greater than zero.");
    }
}
