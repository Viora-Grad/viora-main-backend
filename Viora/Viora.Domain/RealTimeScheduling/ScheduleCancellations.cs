using Viora.Domain.Abstractions;
using Viora.Domain.Users.Customers;

namespace Viora.Domain.RealTimeScheduling;

public class ScheduleCancellations : Entity
{
    public Guid CustomerId { get; private set; }
    public Guid AppointmentId { get; private set; }
    public DateTime ReservationDate { get; private set; }
    public DateTime CancellationDate { get; private set; }
    public string Reason { get; private set; }
    public Customer Customer { get; private set; }
    //public Appointment Appointment { get; private set; }

    public ScheduleCancellations()
    {
        // For EF Core
    }
    private ScheduleCancellations(Guid id, Guid customerId, Guid appointmentId, DateTime reservationDate, DateTime cancellationDate, string reason) : base(id)
    {
        CustomerId = customerId;
        AppointmentId = appointmentId;
        ReservationDate = reservationDate;
        CancellationDate = cancellationDate;
        Reason = reason;
    }

}
