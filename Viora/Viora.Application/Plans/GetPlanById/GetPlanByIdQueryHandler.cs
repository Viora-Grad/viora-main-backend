using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Plans.DTO;
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
    ILimitedFeatureRepository limitedFeatureRepository) : IQueryHandler<GetPlanByIdQuery, PlanDTO>
{
    public async Task<Result<PlanDTO>> Handle(GetPlanByIdQuery request, CancellationToken cancellationToken)
    {
        var plan = await planRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Plan with ID {request.Id} not found.");
        var planFeatures = await planFeatureRepository.GetByPlanIdAsync(request.Id, cancellationToken);
        var featureIds = planFeatures.Select(pf => pf.FeatureId).ToList();
        var limitedFeatureIds = planFeatures.Select(pf => pf.LimitedFeatureId).ToList();
        var features = await featureRepository.GetByIdsAsync(featureIds, cancellationToken);
        var limitedFeatures = await limitedFeatureRepository.GetByIdsAsync(limitedFeatureIds, cancellationToken);

        var featureDTOs = features.Select(f => new FeatureDTO(
               f.Id,
               f.FeatureKey.value,
               f.Description.value
            )
        ).ToList();
        var limitedFeatureDTOs = limitedFeatures.Select(lf => new LimitedFeatureDTO(
            lf.Id,
            lf.Key.value,
            lf.Description.value,
            lf.Limit
            )
        ).ToList();
        var planDTO = PlanDTO.MapToDTO(plan, featureDTOs, limitedFeatureDTOs);
        return Result.Success(planDTO);

    }
}
