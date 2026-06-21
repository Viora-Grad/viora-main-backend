using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Plans.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Plans.Features;

namespace Viora.Application.Plans.GetLimitedFeature;

internal class GetLimitedFeatureByIdQueryHandler(
    ILimitedFeatureRepository limitedFeatureRepository) : ICommandHandler<GetLimitedFeatureByIdQuery, FeatureResponse>
{
    public async Task<Result<FeatureResponse>> Handle(GetLimitedFeatureByIdQuery request, CancellationToken cancellationToken)
    {
        var limitedFeature = await limitedFeatureRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Limited feature with id {request.Id} not found.");
        var LimitedFeatureResponse = new FeatureResponse(
            limitedFeature.Id,
            limitedFeature.Key.value,
            limitedFeature.Description.value
            );

        return Result.Success(LimitedFeatureResponse);
    }
}
