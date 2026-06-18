using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Plans.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Features;

namespace Viora.Application.Plans.GetPlans;

/// <summary>
/// Handles retrieval of all available subscription plans.
/// 
/// Responsibilities:
/// - Fetches all plans from the data source.
/// - Returns plan details including pricing and feature configurations.
/// 
/// Notes:
/// - Read-only operation (no side effects).
/// - Used for displaying available plans to clients.
/// </summary>

public class GetPlansQueryHandler(
    IPlanFeatureRepository planFeatureRepository,
    IPlanRepository planRepository,
    IFeatureRepository featureRepository,
    ILimitedFeatureRepository limitedFeatureRepository) : IQueryHandler<GetPlansQuery, List<PlanResponse>>
{
    public async Task<Result<List<PlanResponse>>> Handle(GetPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = await planRepository.GetAllAsNoTrackingAsync(cancellationToken);
        if (!plans.Any())
            throw new NotFoundException("No plans found.");
        /*
        var planIds = plans.Select(p => p.Id).ToList();

        var planFeatures = await planFeatureRepository.GetByPlanIdsAsync(planIds, cancellationToken);
        var featureIds = planFeatures.Select(pf => pf.FeatureId).Where(id => id.HasValue).Select(id => id.Value).Distinct().ToList();
        var limitedFeatureIds = planFeatures.Select(pf => pf.LimitedFeatureId).Where(id => id.HasValue).Select(id => id.Value).Distinct().ToList();

        var features = await featureRepository.GetByIdsAsync(featureIds, cancellationToken);
        var limitedFeatures = await limitedFeatureRepository.GetByIdsAsync(limitedFeatureIds, cancellationToken);

        // 2. Build lookup dictionaries — O(1) access instead of repeated queries
        var featureLookup = features.ToDictionary(f => f.Id);
        var limitedFeatureLookup = limitedFeatures.ToDictionary(lf => lf.Id);
        var planFeatureLookup = planFeatures.GroupBy(pf => pf.PlanId)
                                               .ToDictionary(g => g.Key, g => g.ToList());
*/
        // 3. Map in memory
        /*var planDtos = plans.Select(plan =>
        {
            var features = planFeatureLookup.GetValueOrDefault(plan.Id, []);
            var featureDtos = features
                                    .Where(pf => pf.FeatureId.HasValue && featureLookup.ContainsKey(pf.FeatureId.Value))
                                    .Select(pf => FeatureResponse.MapToDTO(featureLookup[pf.FeatureId.Value]))
                                    .ToList();
            var limitedFeatureDtos = features
                                    .Where(pf => pf.LimitedFeatureId.HasValue && limitedFeatureLookup.ContainsKey(pf.LimitedFeatureId.Value))
                                    .Select(pf => LimitedFeatureResponse.MapToDTO(limitedFeatureLookup[pf.LimitedFeatureId.Value]))
                                    .ToList();

            return PlanResponse.MapToDTO(plan, featureDtos, limitedFeatureDtos);
        }).ToList();*/

        var planDtos = plans.Select(plan =>
        {

            var featureDTOs = plan.PlanFeatures
            .SelectMany(pf => pf.features)
            .Select(f => new FeatureResponse(
                f.Id,
                f.FeatureKey.ToString(),
                f.Description.ToString()
            )).ToList();

            var limitedFeatureDTOs = plan.PlanLimitedFeatures
                .SelectMany(
                    plf => plf.LimitedFeatures,
                    (plf, lf) => new LimitedFeatureResponse(
                        lf.Id,
                        lf.Key.ToString(),
                        lf.Description.ToString(),
                        plf.LimitValue
                )).ToList();
            return PlanResponse.MapToDTO(plan, featureDTOs, limitedFeatureDTOs);
        }).ToList();

        return Result.Success(planDtos);
    }
}
