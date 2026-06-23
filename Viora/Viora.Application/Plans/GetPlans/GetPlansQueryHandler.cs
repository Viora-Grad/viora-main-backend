using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Plans.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Plans;

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
    IPlanRepository planRepository) : IQueryHandler<GetPlansQuery, List<PlanResponse>>
{
    public async Task<Result<List<PlanResponse>>> Handle(GetPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = await planRepository.GetAllAsNoTrackingAsync(cancellationToken);
        if (!plans.Any())
            throw new NotFoundException("No plans found.");

        var planDtos = plans.Select(plan =>
        {

            var featureDTOs = plan.PlanFeatures
            .Select(pf => new FeatureResponse(
                 pf.Feature.Id,
                 pf.Feature.FeatureKey.value,
                 pf.Feature.Description.value
             )).ToList();

            var limitedFeatureDTOs = plan.PlanLimitedFeatures
            .Select(plf => new LimitedFeatureResponse(
                   plf.LimitedFeature.Id,
                   plf.LimitedFeature.Key.value,
                   plf.LimitedFeature.Description.value,
                   plf.LimitValue
           )).ToList();
            return PlanResponse.MapToDTO(plan, featureDTOs, limitedFeatureDTOs);
        }).ToList();

        return Result.Success(planDtos);
    }
}
