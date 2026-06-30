using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Forms;
using Viora.Domain.Services;
using Viora.Domain.Staffs;

namespace Viora.Application.Forms.CreateForm;

public class CreateFormCommandHandler(
    IStaffRepository staffRepository,
    IFormRepository formRepository,
    IServiceRepository serviceRepository,
    IUnitOfWork unitOfWork
    ) : ICommandHandler<CreateFormCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateFormCommand request, CancellationToken cancellationToken)
    {

        var staff = await staffRepository.GetByIdAsync(request.StaffId, cancellationToken)
            ?? throw new NotFoundException($"the staff with id {request.StaffId} not found");

        var service = await serviceRepository.GetByIdAsync(request.ServiceId, cancellationToken)
            ?? throw new NotFoundException($"the service with id {request.ServiceId} not Found");

        var serviceForm = await formRepository.GetServiceFormAsync(request.ServiceId, cancellationToken);

        if (serviceForm != null)
            return Result.Failure<Guid>(FormError.FormConflict);

        var newForm = Form.Create(request.ServiceId, request.StaffId, request.Name, request.Fields);

        formRepository.Add(newForm.Value);
        await unitOfWork.SaveChangesAsync();
        return Result.Success(newForm.Value.Id);
    }
}
