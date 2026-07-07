using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Branches;
using Viora.Domain.Services;
using Viora.Domain.Staffs;

namespace Viora.Application.Staffs.AssignServices;

public class AssignServicesCommandHandler(
    IServiceRepository serviceRepository,
    IBranchRepository branchRepository,
    IStaffRepository staffRepository,
    IUnitOfWork unitOfWork
    ) : ICommandHandler<AssignServicesCommand>
{
    public async Task<Result> Handle(AssignServicesCommand request, CancellationToken cancellationToken)
    {
        List<Service> services = new();
        foreach (var serviceId in request.ServiceIds)
        {
            var service = await serviceRepository.GetByIdAsync(serviceId, cancellationToken);
            if (service is null)
                throw new NotFoundException($"Service with ID {serviceId} not found.");

            services.Add(service);
        }
        // make sure all services belong to the same organization as the staff
        var branches = new List<Branch>();
        foreach (var service in services)
        {
            var branch = await branchRepository.GetByIdAsync(service.BranchId, cancellationToken);
            if (branch is null)
                throw new NotFoundException($"Branch for Service with ID {service.Id} not found.");
            branches.Add(branch);
        }
        var organizationIds = branches.Select(b => b.OrganizationId).Distinct().ToList();
        if (organizationIds.Count != 1)
        {
            throw new UnauthorizedAccessException("Must Access organizations Services Only");
        }
        var staff = await staffRepository.GetByIdAsync(request.StaffId, cancellationToken);
        if (staff is null)
            throw new NotFoundException($"Staff with ID {request.StaffId} not found.");

        staff.AssignServices(services);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}