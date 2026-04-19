using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Plans.DTO;
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
    ILimitedFeatureRepository limitedFeatureRepository) : IQueryHandler<GetPlansQuery, List<PlanDTO>>
{
    public async Task<Result<List<PlanDTO>>> Handle(GetPlansQuery request, CancellationToken cancellationToken)
    {
        // 1. Fetch all data upfront — 4 DB calls total, not N*M
        var plans = await planRepository.GetAllAsync(cancellationToken);
        if (!plans.Any())
            throw new NotFoundException("No plans found.");

        var planIds = plans.Select(p => p.Id).ToList();

        var planFeatures = await planFeatureRepository.GetByPlanIdsAsync(planIds, cancellationToken);
        var featureIds = planFeatures.Select(pf => pf.FeatureId).Distinct().ToList();
        var limitedFeatureIds = planFeatures.Select(pf => pf.LimitedFeatureId).Distinct().ToList();

        var features = await featureRepository.GetByIdsAsync(featureIds, cancellationToken);
        var limitedFeatures = await limitedFeatureRepository.GetByIdsAsync(limitedFeatureIds, cancellationToken);

        // 2. Build lookup dictionaries — O(1) access instead of repeated queries
        var featureLookup = features.ToDictionary(f => f.Id);
        var limitedFeatureLookup = limitedFeatures.ToDictionary(lf => lf.Id);
        var planFeatureLookup = planFeatures.GroupBy(pf => pf.PlanId)
                                               .ToDictionary(g => g.Key, g => g.ToList());

        // 3. Map in memory
        var planDtos = plans.Select(plan =>
        {
            var features = planFeatureLookup.GetValueOrDefault(plan.Id, []);
            var featureDtos = features
                                    .Where(pf => featureLookup.ContainsKey(pf.FeatureId))
                                    .Select(pf => FeatureDTO.MapToDTO(featureLookup[pf.FeatureId]))
                                    .ToList();
            var limitedFeatureDtos = features
                                    .Where(pf => limitedFeatureLookup.ContainsKey(pf.LimitedFeatureId))
                                    .Select(pf => LimitedFeatureDTO.MapToDTO(limitedFeatureLookup[pf.LimitedFeatureId]))
                                    .ToList();

            return PlanDTO.MapToDTO(plan, featureDtos, limitedFeatureDtos);
        }).ToList();

        return Result.Success(planDtos);
    }
}
