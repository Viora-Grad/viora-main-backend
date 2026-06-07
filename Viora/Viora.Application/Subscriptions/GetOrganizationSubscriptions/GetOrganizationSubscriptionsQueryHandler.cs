using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Subscriptions;

namespace Viora.Application.Subscriptions.GetOrganizationSubscriptions;

public class GetOrganizationSubscriptionsQueryHandler(
    ISubscriptionRepository subscriptionRepository,
    IOrganizationRepository organizationRepository) : IQueryHandler<GetOrganizationSubscriptionsQuery, List<SubscriptionResponse>>
{
    public async Task<Result<List<SubscriptionResponse>>> Handle(GetOrganizationSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        var organizations = await organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"the organization with id {request.OrganizationId} not found");

        var subscriptions = await subscriptionRepository.GetAllByOrganizationIdAsync(request.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"the organization with id {request.OrganizationId} doesn't have subscriptions");

        var result = subscriptions.Select(s => SubscriptionResponse.MapToDto(s)).ToList();

        return Result.Success(result);
    }

}
