using Viora.Domain.Organizations.Suspensions;

namespace Viora.Infrastructure.Settings;

public class SuspensionSettings : ISuspensionSettings
{
    public TimeSpan DeletionSpan { get; set; }
}
