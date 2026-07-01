using FluentValidation;

namespace Viora.Application.Services.GetServices;

internal sealed class GetServicesQueryValidator : AbstractValidator<GetServicesQuery>
{
    public GetServicesQueryValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
    }
}
