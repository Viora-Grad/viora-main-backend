using FluentValidation;
using Viora.Application.Abstractions.Clock;

namespace Viora.Application.Appointments.CreateAppointment;

public class CreateAppointmentValidator : AbstractValidator<CreateAppointmentCommand>
{
    private readonly IDateTimeProvider _dateTimeProvider;
    public CreateAppointmentValidator(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
        RuleFor(x => x.ServiceId).NotEmpty().WithMessage("Service ID is required.");
        RuleFor(x => x.StaffId).NotEmpty().WithMessage("Staff ID is required.");
        RuleFor(x => x.CreatedBy).IsInEnum().WithMessage("CreatedBy is required.");
        RuleFor(x => x.RequestPlatform).IsInEnum().WithMessage("Request platform is required.");
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue).WithMessage("Invalid customer status.");
        RuleFor(x => x.ReservationDate).GreaterThan(_dateTimeProvider.UtcNow).WithMessage("Reservation date must be in the future.");
        RuleFor(x => x.EstimatedDuration).GreaterThan(TimeSpan.Zero).WithMessage("Estimated duration must be greater than zero.");
    }
}
