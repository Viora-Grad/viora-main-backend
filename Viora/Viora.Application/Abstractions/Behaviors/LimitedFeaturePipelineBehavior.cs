using MediatR;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Interfaces;
using Viora.Domain.Organizations;
using Viora.Domain.Plans.Services;

namespace Viora.Application.Abstractions.Behaviors;

public sealed class LimitedFeaturePipelineBehavior<TRequest, TResponse>(
    IOrganizationRepository organizationRepository,
    ILimitedFeatureUsageService limitedFeatureUsageService) :
    IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ILimitedFeatureRequest limitedFeatureRequest)
        {
            return await next();
        }
        var organization = await organizationRepository.GetByIdAsync(limitedFeatureRequest.organizationId, cancellationToken)
            ?? throw new NotFoundException($"Organization with id {limitedFeatureRequest.organizationId} not found.");
        var checkResult = await limitedFeatureUsageService.CheckLimitAsync(
            limitedFeatureRequest.organizationId,
            limitedFeatureRequest.LimitedFeatureId,
            cancellationToken);
        if (checkResult.IsFailure)
            throw new
                QuotaExceededException(
                $"Organization with id " +
                $"{limitedFeatureRequest.organizationId} " +
                $"has exceeded its quota for feature {limitedFeatureRequest.LimitedFeatureId}.");
        await limitedFeatureUsageService.IncrementUsageAsync(
            limitedFeatureRequest.organizationId,
            limitedFeatureRequest.LimitedFeatureId,
            cancellationToken);
        return await next();
    }
}
