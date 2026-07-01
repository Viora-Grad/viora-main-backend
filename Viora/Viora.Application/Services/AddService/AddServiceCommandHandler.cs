using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Services;
using Viora.Domain.Shared;

namespace Viora.Application.Services.AddService;

internal sealed class AddServiceCommandHandler(
    IServiceRepository serviceRepository,
    IBranchRepository branchRepository,
    IOrganizationRepository organizationRepository,
    IServiceSettings serviceSettings,
    IUnitOfWork unitOfWork) : ICommandHandler<AddServiceCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddServiceCommand request, CancellationToken cancellationToken)
    {
        var branch = await branchRepository.GetByIdAsync(request.BranchId, cancellationToken)
            ?? throw new NotFoundException($"Branch {request.BranchId} was not found");

        var organization = await organizationRepository.GetByIdAsync(branch.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"Organization {branch.OrganizationId} was not found");

        var serviceTypeResult = OfferedServiceType.Resolve(request.ServiceType, organization);
        if (serviceTypeResult.IsFailure)
            return Result.Failure<Guid>(serviceTypeResult.Error);

        var serviceResult = Service.Create(
            request.BranchId,
            request.Name,
            request.Description,
            (int)request.Duration.TotalMinutes,
            serviceTypeResult.Value,
            request.Cost,
            serviceSettings);

        if (serviceResult.IsFailure)
            return Result.Failure<Guid>(serviceResult.Error);

        serviceRepository.Add(serviceResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(serviceResult.Value.Id);
    }
}
