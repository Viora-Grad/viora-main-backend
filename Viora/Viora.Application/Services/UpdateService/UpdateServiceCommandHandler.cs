using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Services;

namespace Viora.Application.Services.UpdateService;

internal sealed class UpdateServiceCommandHandler(
    IServiceRepository serviceRepository,
    IBranchRepository branchRepository,
    IOrganizationRepository organizationRepository,
    IServiceSettings serviceSettings,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateServiceCommand>
{
    public async Task<Result> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await serviceRepository.GetByIdAsync(request.ServiceId, cancellationToken);
        if (service is null)
            return Result.Failure(ServiceErrors.NotFound);

        var branch = await branchRepository.GetByIdAsync(service.BranchId, cancellationToken)
            ?? throw new NotFoundException($"Branch {service.BranchId} was not found");

        var organization = await organizationRepository.GetByIdAsync(branch.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"Organization {branch.OrganizationId} was not found");

        var serviceTypeResult = OfferedServiceType.Resolve(request.ServiceType, organization);
        if (serviceTypeResult.IsFailure)
            return serviceTypeResult;

        var updateResult = service.Update(
            request.Name,
            request.Description,
            (int)request.Duration.TotalMinutes,
            serviceTypeResult.Value,
            request.Cost,
            serviceSettings);

        if (updateResult.IsFailure)
            return updateResult;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
