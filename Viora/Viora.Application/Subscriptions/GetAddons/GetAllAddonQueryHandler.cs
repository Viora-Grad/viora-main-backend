using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Plans.DTO;
using Viora.Domain.Abstractions;
using Viora.Domain.Subscriptions.Addons;

namespace Viora.Application.Subscriptions.GetFeatureAddon;

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

public class GetAllAddonQueryHandler(
    ILimitedFeatureAddonRepository limitedFeatureAddonRepository) : IQueryHandler<GetAllAddonsQuery, List<FeatureAddonDTO>>
{
    public async Task<Result<List<FeatureAddonDTO>>> Handle(GetAllAddonsQuery request, CancellationToken cancellationToken)
    {
        var addons = await limitedFeatureAddonRepository.GetAllAsync(cancellationToken);
        if (addons is null || !addons.Any())
            throw new NotFoundException("there are not addons");
        var addonsDto = addons.Select(a => FeatureAddonDTO.MapToDto(a)).ToList();
        return Result.Success(addonsDto);
    }
}
