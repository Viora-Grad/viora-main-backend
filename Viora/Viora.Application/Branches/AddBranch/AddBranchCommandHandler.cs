using NetTopologySuite.Geometries;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.Branches.Internals;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Shared;
using Viora.Domain.Shared.Internal;

namespace Viora.Application.Branches.AddBranch;

internal sealed class AddBranchCommandHandler(
    IOrganizationRepository organizationRepository,
    IBranchRepository branchRepository,
    IDateTimeProvider dateTime,
    IUnitOfWork unitOfWork) : ICommandHandler<AddBranchCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddBranchCommand request, CancellationToken cancellationToken)
    {
        if (!await organizationRepository.ExistsAsync(request.OrganizationId, cancellationToken))
            throw new NotFoundException($"Organization with id {request.OrganizationId} not found.");

        var address = new Address(
            request.AddressNumber,
            request.AddressStreet,
            request.AddressCity,
            request.AddressState,
            request.AddressCountryId,
            request.AddressPostalCode);

        var location = new Point(request.Longitude, request.Latitude) { SRID = 4326 };

        var branch = Branch.Create(
            request.OrganizationId,
            address,
            location,
            new Email(request.ContactEmail),
            request.ServicesProvided.Select(ServiceType.FromValue).ToList(),
            dateTime.UtcNow,
            request.TimeZoneId);

        branchRepository.Add(branch);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(branch.Id);
    }
}
