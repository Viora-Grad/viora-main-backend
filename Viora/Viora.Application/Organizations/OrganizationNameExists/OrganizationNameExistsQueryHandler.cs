using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.OnBoardings;
using Viora.Domain.Organizations.OrganizationDetails;

namespace Viora.Application.Organizations.OrganizationNameExists;

internal class OrganizationNameExistsQueryHandler(
    IOrganizationRepository organizationRepository,
    IOrganizationApplicationRepository applicationRepository) : IQueryHandler<OrganizationNameExistsQuery, bool>
{
    public async Task<Result<bool>> Handle(OrganizationNameExistsQuery request, CancellationToken cancellationToken)
    {
        var existsOrg = await organizationRepository.NameExistsAsync(request.Name, cancellationToken);
        var existsApplication = await applicationRepository.NameExistsAsync(request.Name, cancellationToken);

        return Result.Success(existsApplication || existsOrg);
    }
}
