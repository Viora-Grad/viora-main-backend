namespace Viora.Domain.Plans.Features;

public interface ILimitedFeatureRepository
{
    public Task<LimitedFeature> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    public Task<List<LimitedFeature>> GetByIdsAsync(List<Guid> ids, CancellationToken cancellationToken);

}
