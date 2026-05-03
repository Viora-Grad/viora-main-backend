namespace Viora.Domain.Organizations.Suspensions;

public interface ISuspensionSettings
{
    public TimeSpan DeletionSpan { get; }
}
