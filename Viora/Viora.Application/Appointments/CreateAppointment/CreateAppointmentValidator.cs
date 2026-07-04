using FluentValidation;
using Viora.Application.Abstractions.Clock;
using Viora.Domain.Appointments.Internal;

namespace Viora.Application.Appointments.CreateAppointment;

public class CreateAppointmentValidator : AbstractValidator<CreateAppointmentCommand>
{
    private readonly IDateTimeProvider _dateTimeProvider;
    public CreateAppointmentValidator(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
        RuleFor(x => x.ServiceId).NotEmpty().WithMessage("Service ID is required.");
        RuleFor(x => x.StaffId).NotEmpty().WithMessage("Staff ID is required.");

        RuleFor(x => x.CreatedBy)
            .NotEmpty().
            WithMessage("Created by is required.")
            .Must(x => Enum.TryParse<Creator>(x, true, out _));

        RuleFor(x => x.RequestPlatform)
            .NotEmpty()
            .WithMessage("Request platform is required.")
            .Must(x => Enum.TryParse<Platform>(x, true, out _))
            .WithMessage("Invalid request platform.");

        RuleFor(x => x.Status)
            .Must(x => x == null || Enum.TryParse<CustomerStatus>(x, true, out _))
            .WithMessage("Invalid status value.");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .WithMessage("Payment method is required.")
            .Must(x => Enum.TryParse<PaymentMethod>(x, true, out _))
            .WithMessage("Invalid payment method.");

        RuleFor(a => a.PaymentMethod)
            .Equal("Cash")
            .When(a => a.PaymentId == null)
            .WithMessage("Payment method must be Cash when no payment ID is provided.");

        RuleFor(x => x.ReservationDate).GreaterThan(_dateTimeProvider.UtcNow).WithMessage("Reservation date must be in the future.");
    }
}
