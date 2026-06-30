using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Shared;

namespace Viora.Application.Organizations.UpdateOrganizationProfile;

internal sealed class UpdateOrganizationProfileCommandHandler(
    IOrganizationRepository organizationRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateOrganizationProfileCommand>
{
    public async Task<Result> Handle(UpdateOrganizationProfileCommand request, CancellationToken cancellationToken)
    {
        var organization = await organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"Organization with id {request.OrganizationId} not found.");

        // Subdomain is the public URI identifier (e.g. nile-river.viora.com) — must be globally unique.
        if (await organizationRepository.SubDomainExistsAsync(request.SubDomain, request.OrganizationId, cancellationToken))
            return Result.Failure(OrganizationErrors.SubDomainTaken);

        var servicesProvided = request.ServicesProvided.Select(ServiceType.FromValue).ToList();

        var result = organization.UpdateProfile(
            request.SubDomain,
            request.SupportEmail,
            request.BillingEmail,
            request.ServiceDescription,
            servicesProvided,
            request.About);

        if (result.IsFailure)
            return result;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
