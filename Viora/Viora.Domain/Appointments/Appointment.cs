using Viora.Domain.Abstractions;
using Viora.Domain.Appointments.Events;
using Viora.Domain.Appointments.Internal;
using Viora.Domain.MedicalRecords;
using Viora.Domain.Users.Customers;

namespace Viora.Domain.Appointments;

/// <summary>
/// Represents a customer's appointment for a service with a staff member at a specific date and time.
/// </summary>
/// 
public sealed class Appointment : Entity
{
    public Guid CustomerId { get; private set; }
    public Guid ServiceId { get; private set; }
    public Guid StaffId { get; private set; }
    public DateTime ReservationDate { get; private set; }
    public CustomerStatus Status { get; private set; }
    public bool IsCheckedIn { get; private set; } = false;
    public Creator CreatedBy { get; private set; }
    public Platform RequestPlatform { get; private set; }
    public TimeSpan EstimatedDuration { get; private set; }

    public Customer Customer { get; private set; } = null!; // Navigation property
    public MedicalRecord? MedicalRecord => Customer.MedicalRecord; // Convenience property to access the customer's medical record (should it be used like this ?)
    private Appointment() { } // For EF Core
    private Appointment(Guid id,
        Guid customerId,
        Guid serviceId,
        Guid staffId,
        DateTime reservationDate,
        CustomerStatus status,
        Creator createdBy,
        Platform requestPlatform,
        TimeSpan estimatedDuration) : base(id)
    {
        CustomerId = customerId;
        ServiceId = serviceId;
        StaffId = staffId;
        ReservationDate = reservationDate;
        Status = status;
        CreatedBy = createdBy;
        RequestPlatform = requestPlatform;
        EstimatedDuration = estimatedDuration;
    }

    public static Appointment Book(Guid customerId,
        Guid serviceId,
        Guid staffId,
        DateTime reservationDate,
        CustomerStatus? status,
        Creator createdBy,
        Platform requestPlatform,
        TimeSpan estimatedDuration)
    {
        var appointmentStatus = status ?? CustomerStatus.NotArrived;
        var appointment = new Appointment(Guid.NewGuid(),
            customerId,
            serviceId,
            staffId,
            reservationDate,
            appointmentStatus,
            createdBy,
            requestPlatform,
            estimatedDuration);

        appointment.RaiseDomainEvent(new AppointmentBookedEvent(appointment.Id, reservationDate)); // triggers the background job to send a notification to the customer about the appointment booking
        return appointment;
    }
    public Result CheckIn()
    {
        // Only allow check-in if the customer has not arrived yet
        // TODO: Publish an event when the customer checks in
        if (Status != CustomerStatus.NotArrived)
            return Result.Failure(AppointmentErrors.CheckInProhibited);

        IsCheckedIn = true;
        Status = CustomerStatus.Waiting;
        return Result.Success();
    }
    public Result Start()
    {
        // Only allow starting the appointment if the customer is waiting
        if (Status != CustomerStatus.Waiting)
            return Result.Failure(AppointmentErrors.StartProhibited);
        Status = CustomerStatus.InProgress;
        return Result.Success();
    }
    public Result Complete()
    {
        // Only allow completing the appointment if it is in progress
        if (Status != CustomerStatus.InProgress)
            return Result.Failure(AppointmentErrors.CompleteProhibited);
        Status = CustomerStatus.Completed;
        return Result.Success();
    }
    public Result Delay(TimeSpan delay)
    {
        // Only allow delaying the appointment if it is not completed or in progress
        if (Status == CustomerStatus.Completed || Status == CustomerStatus.InProgress)
            return Result.Failure(AppointmentErrors.DelayProhibited);

        var originalDate = ReservationDate;
        ReservationDate = ReservationDate.Add(delay);
        RaiseDomainEvent(new AppointmentDelayedEvent(Id, originalDate, ReservationDate, delay));
        return Result.Success();
    }
    public Result NoShow()
    {
        // Only allow marking as no-show if the customer has not arrived yet
        if (Status != CustomerStatus.NotArrived)
            return Result.Failure(AppointmentErrors.NoShowProhibited);
        Status = CustomerStatus.NoShow;
        RaiseDomainEvent(new AppointmentNoShowEvent(Id, ReservationDate));
        return Result.Success();
    }
}
