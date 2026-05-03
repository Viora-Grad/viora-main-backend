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
}
