using Viora.Domain.Abstractions;
using Viora.Domain.Appointments.Events;
using Viora.Domain.Appointments.Internal;
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
    public DateTime EndTime => ReservationDate.Add(EstimatedDuration); // Convenience property to calculate the end time of the appointment

    public DateTime CreatedAt { get; private set; }
    public DateTime? LastUpdatedAt { get; private set; }

    public Customer Customer { get; private set; } = null!; // Navigation property
    private Appointment() { } // For EF Core
    private Appointment(Guid id,
        Guid customerId,
        Guid serviceId,
        Guid staffId,
        DateTime reservationDate,
        CustomerStatus status,
        Creator createdBy,
        Platform requestPlatform,
        TimeSpan estimatedDuration,
        DateTime createdAt) : base(id)
    {
        CustomerId = customerId;
        ServiceId = serviceId;
        StaffId = staffId;
        ReservationDate = reservationDate;
        Status = status;
        CreatedBy = createdBy;
        RequestPlatform = requestPlatform;
        EstimatedDuration = estimatedDuration;
        CreatedAt = createdAt;
    }

    public static Appointment Book(Guid customerId,
        Guid serviceId,
        Guid staffId,
        DateTime reservationDate,
        CustomerStatus? status,
        Creator createdBy,
        Platform requestPlatform,
        TimeSpan estimatedDuration,
        DateTime createdAt)
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
            estimatedDuration,
            createdAt);

        appointment.RaiseDomainEvent(new AppointmentBookedEvent(appointment.Id, reservationDate)); // triggers the background job to send a notification to the customer about the appointment booking
        return appointment;
    }
    public Result CheckIn(DateTime checkInTime)
    {
        // Only allow check-in if the customer has not arrived yet
        // TODO: Publish an event when the customer checks in
        if (Status != CustomerStatus.NotArrived)
            return Result.Failure(AppointmentErrors.CheckInProhibited);

        IsCheckedIn = true;
        Status = CustomerStatus.Waiting;
        LastUpdatedAt = checkInTime;

        RaiseDomainEvent(new AppointmentCheckedInEvent(Id, checkInTime)); // triggers the background job to send a notification to the staff about the customer check-in
        return Result.Success();
    }
    public Result Start(DateTime startTime)
    {
        // Only allow starting the appointment if the customer is waiting
        if (Status != CustomerStatus.Waiting)
            return Result.Failure(AppointmentErrors.StartProhibited);
        Status = CustomerStatus.InProgress;
        LastUpdatedAt = startTime;
        return Result.Success();
    }
    public Result Complete(DateTime completeTime)
    {
        // Only allow completing the appointment if it is in progress
        if (Status != CustomerStatus.InProgress)
            return Result.Failure(AppointmentErrors.CompleteProhibited);
        Status = CustomerStatus.Completed;
        LastUpdatedAt = completeTime;

        RaiseDomainEvent(new AppointmentCompletedEvent(Id, completeTime)); // triggers the background job to send a notification to the customer about the appointment completion and request feedback
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
    public Result NoShow(DateTime noShowTime)
    {
        // Only allow marking as no-show if the customer has not arrived yet
        if (Status != CustomerStatus.NotArrived)
            return Result.Failure(AppointmentErrors.NoShowProhibited);

        if (noShowTime < ReservationDate)
            return Result.Failure(AppointmentErrors.NoShowTimeInvalid);

        Status = CustomerStatus.NoShow;
        LastUpdatedAt = noShowTime;
        RaiseDomainEvent(new AppointmentNoShowEvent(Id, ReservationDate));
        return Result.Success();
    }
    public Result Cancel(DateTime cancelTime)
    {
        // Only allow canceling the appointment if it is not completed
        if (Status == CustomerStatus.Completed || Status == CustomerStatus.InProgress)
            return Result.Failure(AppointmentErrors.CancellationProhibited);
        Status = CustomerStatus.Canceled;
        LastUpdatedAt = cancelTime;
        RaiseDomainEvent(new AppointmentCanceledEvent(Id, ReservationDate));
        return Result.Success();
    }
}


/*
 * builder.Property<byte[]>("RowVersion")
               .IsRowVersion()
               .IsConcurrencyToken()
               .HasColumnName("RowVersion");
*/