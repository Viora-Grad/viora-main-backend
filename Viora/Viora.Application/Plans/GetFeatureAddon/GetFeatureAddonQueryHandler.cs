using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Plans.DTO;
using Viora.Domain.Abstractions;
using Viora.Domain.Plans.Addons;
using Viora.Domain.Plans.Features;

namespace Viora.Application.Plans.GetFeatureAddon;

public class GetFeatureAddonQueryHandler(
    ILimitedFeatureRepository limitedFeatureRepository,
    ILimitedFeatureAddonRepository limitedFeatureAddonRepository) : IQueryHandler<GetFeatureAddonQuery, List<FeatureAddonDTO>>
{
    public async Task<Result<List<FeatureAddonDTO>>> Handle(GetFeatureAddonQuery request, CancellationToken cancellationToken)
    {
        var limitedFeature = await limitedFeatureRepository.GetByIdAsync(request.LimitedFeatureId, cancellationToken)
            ?? throw new NotFoundException($"Limited feature with id {request.LimitedFeatureId} not found.");
        var addon = await limitedFeatureAddonRepository.GetByLimitedFeatureIdAsync(request.LimitedFeatureId, cancellationToken)
            ?? throw new NotFoundException($"Addon for limited feature with id {request.LimitedFeatureId} not found.");
        var addonDTOs = addon.Select(FeatureAddonDTO.MapToDto).ToList();
        return Result.Success(addonDTOs);
    }
}
