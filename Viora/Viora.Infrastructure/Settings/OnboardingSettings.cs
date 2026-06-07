using Viora.Domain.Organizations.OnBoardings;

namespace Viora.Infrastructure.Settings;

public class OnboardingSettings : IOnboardingSettings
{
    public int DaysTillExpiry { get; set; }
    public TimeSpan CoolDownPeriod { get; set; }
}
