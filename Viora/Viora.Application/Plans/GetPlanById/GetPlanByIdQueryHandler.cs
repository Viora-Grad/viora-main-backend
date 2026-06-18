using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Plans.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Plans;

namespace Viora.Application.Plans.GetPlanById;

/// <summary>
/// Handles retrieval of a specific subscription plan by its identifier.
/// 
/// Responsibilities:
/// - Fetches the plan matching the provided ID.
/// - Returns detailed information about the plan and its features.
/// 
/// Notes:
/// - Returns failure if the plan does not exist.
/// - Read-only operation with no domain side effects.
/// </summary>

public class GetPlanByIdQueryHandler(
    IPlanRepository planRepository
) : IQueryHandler<GetPlanByIdQuery, PlanResponse>
{
    public async Task<Result<PlanResponse>> Handle(GetPlanByIdQuery request, CancellationToken cancellationToken)
    {
        var plan = await planRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Plan with ID {request.Id} not found.");

        var featureDTOs = plan.PlanFeatures
            .Select(pf => new FeatureResponse(
                    pf.Feature.Id,
                    pf.Feature.FeatureKey.ToString(),
                    pf.Feature.Description.ToString()
            )).ToList();

        var limitedFeatureDTOs = plan.PlanLimitedFeatures
            .Select(plf => new LimitedFeatureResponse(
                    plf.LimitedFeature.Id,
                    plf.LimitedFeature.Key.ToString(),
                    plf.LimitedFeature.Description.ToString(),
                    plf.LimitValue
            )).ToList();
        var planDTO = PlanResponse.MapToDTO(plan, featureDTOs, limitedFeatureDTOs);
        return Result.Success(planDTO);

    }
}
