namespace Viora.Domain.Plans.Addons;

public interface ILimitedFeatureAddonRepository
{
    Task<LimitedFeatureAddon> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
