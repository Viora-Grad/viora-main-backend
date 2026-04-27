namespace Viora.Domain.Subscriptions.Addons;

public interface ILimitedFeatureAddonRepository
{
    Task<LimitedFeatureAddon?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<LimitedFeatureAddon>> GetByLimitedFeatureIdAsync(Guid limitedFeatureId, CancellationToken cancellationToken);
    Task<List<LimitedFeatureAddon>> GetByIdsAsync(List<Guid> ids, CancellationToken cancellationToken);
    Task<List<LimitedFeatureAddon>> GetAllAsync(CancellationToken cancellationToken);
}
