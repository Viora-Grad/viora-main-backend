using MediatR;
using Viora.Domain.Appointments.Events;

namespace Viora.Application.Appointments.AppointmentCompleted;

public class AppointmentCompletedEventHandler : INotificationHandler<AppointmentCompletedEvent>
{
    public async Task Handle(AppointmentCompletedEvent notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // transfare the money from the wallet from customer wallet to client wallet 

    }
}
