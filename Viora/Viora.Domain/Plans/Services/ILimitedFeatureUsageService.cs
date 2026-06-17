using Viora.Domain.Abstractions;

namespace Viora.Domain.Plans.Services;

public interface ILimitedFeatureUsageService
{
    Task<Result> CheckLimitAsync(Guid organizationId, Guid limitedFeatureId, long delta, CancellationToken cancellationToken);
    Task<Result> ConsumeLimit(Guid organizationId, Guid limitedFeatureId, long delta, CancellationToken cancellationToken);

}
