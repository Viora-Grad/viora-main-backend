namespace Viora.Domain.Plans.Addons;

public interface ILimitedFeatureAddonRepository
{
    Task<LimitedFeatureAddon?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<LimitedFeatureAddon>> GetByLimitedFeatureIdAsync(Guid limitedFeatureId, CancellationToken cancellationToken);
}
