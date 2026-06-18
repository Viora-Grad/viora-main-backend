using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Plans.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Features;

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
    IPlanRepository planRepository,
    IPlanFeatureRepository planFeatureRepository,
    IFeatureRepository featureRepository,
    ILimitedFeatureRepository limitedFeatureRepository) : IQueryHandler<GetPlanByIdQuery, PlanResponse>
{
    public async Task<Result<PlanResponse>> Handle(GetPlanByIdQuery request, CancellationToken cancellationToken)
    {
        var plan = await planRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Plan with ID {request.Id} not found.");

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
        var planDTO = PlanResponse.MapToDTO(plan, featureDTOs, limitedFeatureDTOs);
        return Result.Success(planDTO);

    }
}
