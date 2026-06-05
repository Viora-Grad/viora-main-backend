using MediatR;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Organizations.OrganizationDetails;
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
    where TRequest : notnull, IBaseLimitedFeatureCommand
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (await organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken) is null)
            throw new NotFoundException($"Organization with id {request.OrganizationId} not found.");

        var checkResult = await limitedFeatureUsageService.CheckLimitAsync(
            request.OrganizationId,
            request.LimitedFeatureId,
            request.DeltaChange,
            cancellationToken);

        if (checkResult.IsFailure)
            throw new
                QuotaExceededException(
                $"Organization with id " +
                $"{request.OrganizationId} " +
                $"has exceeded its quota for feature {request.LimitedFeatureId}.");

        var result = await limitedFeatureUsageService.ConsumeLimit(
            request.OrganizationId,
            request.LimitedFeatureId,
            request.DeltaChange,
            cancellationToken
            );

        if (result.IsFailure)
            throw new NotFoundException("this organization does not have this feature");
        return await next(cancellationToken);
    }
}
