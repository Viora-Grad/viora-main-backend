using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Branches.SharedResponses;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.Organizations.OrganizationDetails;

namespace Viora.Application.Branches.GetBranchDetails;

internal class GetBranchDetailsQueryHandler(
    IBranchRepository branchRepository,
    IOrganizationRepository organizationRepository,
    IDateTimeProvider dateTimeProvider) : IQueryHandler<GetBranchDetailsQuery, BranchDetailsResponse>
{
    public async Task<Result<BranchDetailsResponse>> Handle(GetBranchDetailsQuery request, CancellationToken cancellationToken)
    {
        var branch = await branchRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Branch with id {request.Id} not found");

        var organization = await organizationRepository.GetByIdAsync(branch.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"Branch with id {request.Id} not found");

        var response = new BranchDetailsResponse(
            branch.Id,
            organization.Id,
            organization.Name,
            branch.ServicesProvided
                .Select(x => x.Value)
                .ToList()
                .AsReadOnly(),
            branch.Address.Value,
            new Coordinates(branch.Location),
            branch.Status,
            branch.ContactEmail,
            branch.PhoneNumbers
                .Select(x => x.Value)
                .ToList()
                .AsReadOnly(),
            branch.BusinessHours,
            branch.TimeZone,
            branch.OpenedAtUtc,
            branch.Gallery
                .Select(g => new MediaResponse(g.Id, g.MimeType, g.Name, g.UploadedAtUtc))
                .ToList()
                .AsReadOnly(),
            branch.IsCurrentlyOpen(dateTimeProvider.UtcNow)
            );

        return Result.Success(response);
    }
}
