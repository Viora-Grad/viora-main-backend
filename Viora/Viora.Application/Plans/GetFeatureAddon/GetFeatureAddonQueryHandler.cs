using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Plans.DTO;
using Viora.Domain.Abstractions;
using Viora.Domain.Plans.Addons;
using Viora.Domain.Plans.Features;

namespace Viora.Application.Plans.GetFeatureAddon;

/// <summary>
/// Handles retrieval of available add-ons for a specific feature.
/// 
/// Responsibilities:
/// - Returns add-on configurations such as limits, pricing, and availability.
/// 
/// Notes:
/// - Used to determine what add-ons can be purchased for a feature.
/// - Read-only operation (no state changes).
/// </summary>

public class GetFeatureAddonQueryHandler(
    ILimitedFeatureRepository limitedFeatureRepository,
    ILimitedFeatureAddonRepository limitedFeatureAddonRepository) : IQueryHandler<GetFeatureAddonQuery, List<FeatureAddonDTO>>
{
    public async Task<Result<List<FeatureAddonDTO>>> Handle(GetFeatureAddonQuery request, CancellationToken cancellationToken)
    {
        var limitedFeature = await limitedFeatureRepository.GetByIdAsync(request.LimitedFeatureId, cancellationToken)
            ?? throw new NotFoundException($"Limited feature with id {request.LimitedFeatureId} not found.");
        var addon = await limitedFeatureAddonRepository.GetByLimitedFeatureIdAsync(request.LimitedFeatureId, cancellationToken);
        if (addon == null || !addon.Any())
            throw new NotFoundException($"Limited feature with id {request.LimitedFeatureId} does not have Addon");
        var addonDTOs = addon.Select(FeatureAddonDTO.MapToDto).ToList();

        return Result.Success(addonDTOs);
    }
}
