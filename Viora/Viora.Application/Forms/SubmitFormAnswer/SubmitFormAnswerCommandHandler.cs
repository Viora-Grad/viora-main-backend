using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Appointments;
using Viora.Domain.Branches;
using Viora.Domain.Forms;
using Viora.Domain.Services;

namespace Viora.Application.Forms.SubmitFormAnswer;

internal class SubmitFormAnswerCommandHandler(
    IAppointmentsRepository appointmentRepository,
    IUnitOfWork unitOfWork,
    IFormRepository formRepository,
    IFormSubmissionRepository formSubmissionRepository,
    IServiceRepository serviceRepository,
    IBranchRepository branchRepository,
    IDateTimeProvider dateTimeProvider
    ) : ICommandHandler<SubmitFormAnswerCommand, Guid>
{
    public async Task<Result<Guid>> Handle(SubmitFormAnswerCommand request, CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken)
            ?? throw new NotFoundException($"the appointment with id {request.AppointmentId} not found ");

        var form = await formRepository.GetByIdAsync(request.FormId, cancellationToken)
            ?? throw new NotFoundException($"the form with id {request.FormId} not found ");
        var service = await serviceRepository.GetByIdAsync(form.ServiceId, cancellationToken)
            ?? throw new NotFoundException($"the service with id {form.ServiceId} not found");
        var branch = await branchRepository.GetByIdAsync(service.BranchId, cancellationToken)
            ?? throw new NotFoundException($"the branch with id {service.Id} not found");

        var alreadySumbit = await formSubmissionRepository.GetByAppointmentIdAsync(request.AppointmentId, request.FormId, cancellationToken);

        if (alreadySumbit != null)
            return Result.Failure<Guid>(FormSubmissionError.AlreadySubmit);

        var entity = FormSubmission.Create(
            request.AppointmentId,
            form.Id,
            request.Submission,
            dateTimeProvider.UtcNow
            );

        if (entity.IsFailure)
            return Result.Failure<Guid>(entity.Error);

        formSubmissionRepository.Add(entity.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(entity.Value.Id);

    }
}
