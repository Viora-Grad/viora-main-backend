namespace Viora.Domain.Scheduling;

public interface ISchedulingSettings
{
    public int BatchSize { get; }
    public int MaxAttempts { get; }
    public TimeSpan PollInterval { get; }
}
