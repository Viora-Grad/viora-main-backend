using MediatR;
using Microsoft.Extensions.Logging;
using Viora.Application.Abstractions.Mail;
using Viora.Domain.Branches;
using Viora.Domain.Inventory;
using Viora.Domain.Inventory.Events;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Users.Owners;

namespace Viora.Application.Inventories.MinimumThresholdReached;

internal class MinimumThresholdReachedEventHandler(
    IInventoryItemRepository itemRepository,
    IBranchRepository branchRepository,
    IOrganizationRepository organizationRepository,
    IOwnerRepository ownerRepository,
    ILogger<MinimumThresholdReachedEventHandler> logger,
    IEmailSender emailSender) : INotificationHandler<MinimumThresholdReachedEvent>
{
    public async Task Handle(MinimumThresholdReachedEvent notification, CancellationToken cancellationToken)
    {
        const string LogTemplate = "Failed to dispatch event {EventName}: {EntityName} with ID {EntityId} was not found.";

        var branch = await branchRepository.GetByIdAsync(notification.BranchId, cancellationToken);
        if (branch is null)
        {
            logger.LogWarning(LogTemplate, nameof(MinimumThresholdReachedEvent), "Branch", notification.BranchId);
            return;
        }

        var organization = await organizationRepository.GetByIdAsync(branch.OrganizationId, cancellationToken);
        if (organization is null)
        {
            logger.LogWarning(LogTemplate, nameof(MinimumThresholdReachedEvent), "Organization", branch.OrganizationId);
            return;
        }

        var owner = await ownerRepository.GetByIdAsync(organization.OwnerId, cancellationToken);
        if (owner is null)
        {
            logger.LogWarning(LogTemplate, nameof(MinimumThresholdReachedEvent), "Owner", organization.OwnerId);
            return;
        }

        var item = await itemRepository.GetByIdAsync(notification.ItemId, cancellationToken);
        if (item is null)
        {
            logger.LogWarning(LogTemplate, nameof(MinimumThresholdReachedEvent), "Item", notification.ItemId);
            return;
        }

        await emailSender.SendAsync(owner.UserProfile.Email.Value, EmailTemplateFactory.MinimumThresholdReached(item.Name, branch.Address.Value, item.Quantity), cancellationToken);
    }
}
