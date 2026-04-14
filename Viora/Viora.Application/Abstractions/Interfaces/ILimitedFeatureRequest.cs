namespace Viora.Application.Abstractions.Interfaces;

public interface ILimitedFeatureRequest
{
    public Guid organizationId { get; }
    public Guid LimitedFeatureId { get; }

}
