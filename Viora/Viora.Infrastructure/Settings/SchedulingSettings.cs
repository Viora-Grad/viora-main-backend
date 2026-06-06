using Viora.Domain.Scheduling;

namespace Viora.Infrastructure.Settings;

public class SchedulingSettings : ISchedulingSettings
{
    public int BatchSize { get; set; }
    public int MaxAttempts { get; set; }
    public TimeSpan PollInterval { get; set; }
}
