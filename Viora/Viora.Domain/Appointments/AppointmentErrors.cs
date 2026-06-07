using Viora.Domain.Abstractions;

namespace Viora.Domain.Appointments;

public static class AppointmentErrors
{
    public static Error CheckInProhibited =>
        new("Appointment.CheckInProhibited", "Customer has already checked in or appointment is in progress/completed.", ErrorCategory.Validation);
    public static Error StartProhibited =>
        new("Appointment.StartProhibited", "Appointment cannot be started because the customer is not waiting.", ErrorCategory.Validation);
    public static Error CompleteProhibited =>
        new("Appointment.CompleteProhibited", "Appointment cannot be completed because it is not in progress.", ErrorCategory.Validation);
    public static Error DelayProhibited =>
        new("Appointment.DelayProhibited", "Cannot delay a completed appointment.", ErrorCategory.Validation);
    public static Error CancellationProhibited =>
        new("Appointment.CancellationProhibited", "Cannot cancel the appointment", ErrorCategory.Validation);
    public static Error NoShowProhibited =>
        new("Appointment.NoShowProhibited", "Cannot mark an appointment as no-show if the customer has already arrived or the appointment is in progress/completed.", ErrorCategory.Validation);

    public static Error InvalidAppointmentTime =>
        new("Appointment.InvalidAppointmentTime", "The appointment time must be in the future.", ErrorCategory.Validation);

    public static Error AppointmentTimeConflict =>
        new("Appointment.AppointmentTimeConflict", "The requested appointment time conflicts with an existing appointment.", ErrorCategory.Conflict);
    public static Error AppointmentNotFound =>
        new("Appointment.AppointmentNotFound", "The specified appointment was not found.", ErrorCategory.NotFound);
}
