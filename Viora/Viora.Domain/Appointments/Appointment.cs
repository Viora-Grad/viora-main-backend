using Viora.Domain.Abstractions;
using Viora.Domain.Appointments.Events;
using Viora.Domain.Appointments.Internal;
using Viora.Domain.Branches;
using Viora.Domain.Services;
using Viora.Domain.Staffs;
using Viora.Domain.Users.Customers;

namespace Viora.Domain.Appointments;

/// <summary>
/// Represents a customer's appointment for a service with a staff member at a specific date and time.
/// </summary>
/// 
public sealed class Appointment : Entity
{
    public Guid? CustomerId { get; private set; } // nullable if only staff made the appointment for unknown customer
    public Guid ServiceId { get; private set; }
    public Guid StaffId { get; private set; }
    public Guid BranchId { get; private set; }
    public Guid? PaymentId { get; private set; }
    public DateTime ReservationDate { get; private set; }
    public CustomerStatus Status { get; private set; }
    public int AppointmentQueueNumber { get; private set; } // This can be set by the application service when booking the appointment based on the number of existing appointments for the same date, time, and staff member
    public bool IsCheckedIn { get; private set; } = false;
    public Creator CreatedBy { get; private set; }
    public Platform RequestPlatform { get; private set; }
    public TimeSpan EstimatedDuration { get; private set; }
    public DateTime EndTime => ReservationDate.Add(EstimatedDuration); // Convenience property to calculate the end time of the appointment

    public DateTime CreatedAt { get; private set; }
    public DateTime? LastUpdatedAt { get; private set; }

    public Customer? Customer { get; private set; }
    public Service Service { get; private set; } = null!; // Navigation property
    public Staff Staff { get; private set; } = null!; // Navigation property
    public Branch Branch { get; private set; } = null!; // Navigation property
    private Appointment() { } // For EF Core
    private Appointment(Guid id,
        Guid? customerId,
        Guid serviceId,
        Guid staffId,
        Guid branchId,
        Guid? paymentId,
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
        BranchId = branchId;
        PaymentId = paymentId;
        ReservationDate = reservationDate;
        Status = status;
        CreatedBy = createdBy;
        RequestPlatform = requestPlatform;
        EstimatedDuration = estimatedDuration;
        CreatedAt = createdAt;
    }

    public static Appointment Book(
        Guid? customerId,
        Guid serviceId,
        Guid staffId,
        Guid branchId,
        Guid? paymentId,
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
            branchId,
            paymentId,
            reservationDate,
            appointmentStatus,
            createdBy,
            requestPlatform,
            estimatedDuration,
            createdAt);

        appointment.RaiseDomainEvent(new AppointmentBookedEvent(branchId, appointment.Id, reservationDate)); // triggers the background job to send a notification to the customer about the appointment booking
        return appointment;
    }

    public Result CheckIn(DateTime checkInTime, Creator madeBy)
    {
        if (madeBy == Creator.Staff)
        {
            Status = CustomerStatus.InProgress;
            IsCheckedIn = true;
            LastUpdatedAt = checkInTime;
            RaiseDomainEvent(new AppointmentCheckedInEvent(Id, checkInTime)); // triggers the background job to send a notification to the staff about the customer check-in
            return Result.Success();
        }
        // Only allow check-in if the customer has not arrived yet
        if (Status != CustomerStatus.NotArrived)
            return Result.Failure(AppointmentErrors.CheckInProhibited);
        if (checkInTime.AddMinutes(30) < ReservationDate)
            return Result.Failure(AppointmentErrors.CheckInNotWithinAcceptableWindow);
        Status = CustomerStatus.InProgress;
        IsCheckedIn = true;
        LastUpdatedAt = checkInTime;
        RaiseDomainEvent(new AppointmentCheckedInEvent(Id, checkInTime)); // triggers the background job to send a notification to the staff about the customer check-in
        return Result.Success();
    }

    public Result Start(DateTime startTime) // this sorta is not valid state anymore since check-in covers that 
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
        // since it's only done by staff, we can immediately set the status to completed without checking the current status, as the staff would know if the appointment is in progress or not
        Status = CustomerStatus.Completed;
        LastUpdatedAt = completeTime;

        RaiseDomainEvent(new AppointmentCompletedEvent(Id, completeTime, ReservationDate)); // triggers the background job to send a notification to the customer about the appointment completion and request feedback
        return Result.Success();
    }
    public Result Delay(TimeSpan delay, string reason)
    {
        // Only allow delaying the appointment if it is not completed or in progress
        if (Status == CustomerStatus.Completed || Status == CustomerStatus.InProgress)
            return Result.Failure(AppointmentErrors.DelayProhibited);

        var originalDate = ReservationDate;
        ReservationDate = ReservationDate.Add(delay);
        RaiseDomainEvent(new AppointmentDelayedEvent(Id, originalDate, ReservationDate, delay, StaffId, CustomerId, Status.ToString()));
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
    public Result Cancel(DateTime cancelTime, Guid branchId, Creator creator)
    {
        // Only allow canceling the appointment if it is not completed
        if (Status == CustomerStatus.Completed || Status == CustomerStatus.InProgress || cancelTime.AddHours(2) > ReservationDate)
            return Result.Failure(AppointmentErrors.CancellationProhibited);
        Status = CustomerStatus.Canceled;
        LastUpdatedAt = cancelTime;
        RaiseDomainEvent(new AppointmentCanceledEvent(Id, branchId, ReservationDate, creator));
        return Result.Success();
    }


}


/*
 * builder.Property<byte[]>("RowVersion")
               .IsRowVersion()
               .IsConcurrencyToken()
               .HasColumnName("RowVersion");
*/