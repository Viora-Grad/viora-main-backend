namespace Viora.Domain.Plans.Features;

public interface IFeatureRepository
{
    Task<Feature?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Feature>> GetByIdsAsync(List<Guid> ids, CancellationToken cancellationToken);
}
