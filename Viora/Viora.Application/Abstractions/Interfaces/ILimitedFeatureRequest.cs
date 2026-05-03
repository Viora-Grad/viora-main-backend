namespace Viora.Application.Abstractions.Interfaces;

public interface ILimitedFeature
{
    public Guid OrganizationId { get; }
    public Guid LimitedFeatureId { get; }

}
