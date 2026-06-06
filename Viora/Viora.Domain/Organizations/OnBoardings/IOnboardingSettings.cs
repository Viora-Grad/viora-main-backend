namespace Viora.Domain.Organizations.OnBoardings;

public interface IOnboardingSettings
{
    public int DaysTillExpiry { get; }
    public TimeSpan CoolDownPeriod { get; }
}