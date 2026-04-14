using Viora.Domain.Abstractions;

namespace Viora.Domain.Plans.Services;

public interface ILimitedFeatureUsageService
{
    Task<Result> CheckLimitAsync(Guid organizationId, Guid limitedFeatureId, CancellationToken cancellationToken);
    void ConsumeLimit(Guid organizationId, Guid limitedFeatureId, CancellationToken cancellationToken);

}
