using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.OrganizationDetails;

namespace Viora.Application.Organizations.HideOrganization;

internal class HideOrganizationCommandHandler(
    IOrganizationRepository organizationRepository) : ICommandHandler<HideOrganizationCommand>
{
    public async Task<Result> Handle(HideOrganizationCommand request, CancellationToken cancellationToken)
    {
        var organization = await organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"Organization with Id {request.OrganizationId} not fonud");

        var result = organization.Hide();

        if (result.IsSuccess)
            return Result.Failure(result.Error);

        return Result.Success();
    }
}
