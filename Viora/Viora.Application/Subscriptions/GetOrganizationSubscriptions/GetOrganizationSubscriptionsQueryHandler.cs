using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Organizations;
using Viora.Domain.Plans.Features;
using Viora.Domain.Subscriptions;

namespace Viora.Application.Subscriptions.GetOrganizationSubscriptions;

public class GetOrganizationSubscriptionsQueryHandler(
    ISubscriptionRepository subscriptionRepository,
    IOrganizationRepository organizationRepository,
    ILimitedFeatureRepository limitedFeatureRepository) : IQueryHandler<GetOrganizationSubscriptionsQuery, List<SubscriptionDto>>
{
    public async Task<Result<List<SubscriptionDto>>> Handle(GetOrganizationSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        var organizations = await organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"the organization with id {request.OrganizationId} not found");

        var subscriptions = await subscriptionRepository.GetAllByOrganizationIdAsync(request.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"the organization with id {request.OrganizationId} doesn't have subscriptions");

        var result = subscriptions.Select(s => SubscriptionDto.MapToDto(s)).ToList();

        return Result.Success(result);
    }

}
