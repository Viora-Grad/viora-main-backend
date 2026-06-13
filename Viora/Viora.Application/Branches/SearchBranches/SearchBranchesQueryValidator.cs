using FluentValidation;
using Viora.Domain.Shared;

namespace Viora.Application.Branches.SearchBranches;

internal sealed class SearchBranchesQueryValidator : AbstractValidator<SearchBranchesQuery>
{
    public SearchBranchesQueryValidator()
    {
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .When(x => x.Latitude.HasValue);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .When(x => x.Longitude.HasValue);

        RuleFor(x => x.DistanceWithinMeters)
            .GreaterThan(0)
            .When(x => x.DistanceWithinMeters.HasValue);

        RuleFor(x => x.Longitude)
            .NotNull().WithMessage("Both Long and Lat must be added")
            .When(x => x.Latitude != null);

        RuleFor(x => x.Latitude)
            .NotNull().WithMessage("Both Long and Lat must be added")
            .When(x => x.Longitude != null);

        RuleFor(x => x.Latitude)
            .NotNull().WithMessage("Latitude is required when searching by distance.")
            .When(x => x.DistanceWithinMeters.HasValue);

        RuleFor(x => x.Longitude)
            .NotNull().WithMessage("Longitude is required when searching by distance.")
            .When(x => x.DistanceWithinMeters.HasValue);

        RuleFor(x => x.MinimumRating)
            .InclusiveBetween(0.0, 10.0);

        RuleFor(x => x.Page)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleForEach(x => x.ServicesFilter)
            .Must(s => ServiceType.All.Any(t => t.Value.Equals(s, StringComparison.OrdinalIgnoreCase)))
            .WithMessage("One or more service types are invalid.")
            .When(x => x.ServicesFilter != null && x.ServicesFilter.Any());
    }
}
