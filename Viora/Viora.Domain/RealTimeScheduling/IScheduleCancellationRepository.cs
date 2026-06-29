namespace Viora.Domain.RealTimeScheduling;

public interface IScheduleCancellationRepository
{
    public void Add(ScheduleCancellations cancellation);
}
