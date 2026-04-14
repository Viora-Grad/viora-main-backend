using Viora.Domain.Plans.Features;

namespace Viora.Domain.Plans;

public interface ILimitedFeatureRepository
{
    public Task<LimitedFeature> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
