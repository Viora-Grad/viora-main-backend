using MediatR;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Interfaces;
using Viora.Domain.Organizations;
using Viora.Domain.Plans.Services;

namespace Viora.Application.Abstractions.Behaviors;

/// <summary>
/// Pipeline behavior responsible for handling feature usage consumption.
/// 
/// Responsibilities:
/// - Intercepts requests that consume limited features.
/// - Validates that the organization has an active subscription.
/// - Checks if the requested feature usage is within allowed limits.
/// - Prevents execution if limits are exceeded.
/// - Updates feature usage counters when consumption is valid.
/// 
/// Notes:
/// - Acts as a cross-cutting concern applied before request handlers.
/// - Centralizes feature usage validation logic across the system.
/// - Ensures consistent enforcement of feature limits.
/// </summary>

public sealed class LimitedFeaturePipelineBehavior<TRequest, TResponse>(
    IOrganizationRepository organizationRepository,
    ILimitedFeatureUsageService limitedFeatureUsageService) :
    IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ILimitedFeature limitedFeatureRequest)
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
        var result = await limitedFeatureUsageService.ConsumeLimit(
            limitedFeatureRequest.organizationId,
            limitedFeatureRequest.LimitedFeatureId,
            cancellationToken
            );
        if (result.IsFailure)
            throw new NotFoundException("this organization does not have this feature");
        return await next();
    }
}
