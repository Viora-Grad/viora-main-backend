using Moq;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Subscriptions.AddAddon;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Organizations.Shared.Enums;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Features;
using Viora.Domain.Shared;
using Viora.Domain.Subscriptions;
using Viora.Domain.Subscriptions.Addons;
using Viora.Domain.Subscriptions.Addons.Event;
using Viora.Domain.Subscriptions.Addons.Internal;

namespace Viora.Test.Compenents.Application.Subscriptions;

/// <summary>
/// Unit tests for the AddonAddedDomainEventHandler covering idempotency, not-found scenarios, and addon processing.
/// </summary>
[TestClass]
public sealed class AddonAddedDomainEventHandlerTests
{
    private readonly Mock<ISubscriptionRepository> _subscriptionRepoMock = new();
    private readonly Mock<IOrganizationRepository> _organizationRepoMock = new();
    private readonly Mock<ILimitedFeatureAddonRepository> _limitedFeatureAddonRepoMock = new();
    private readonly Mock<IFeatureUsageRepository> _featureUsageRepoMock = new();
    private readonly Mock<IPlanLimitedFeatureRepository> _planLimitedFeatureRepoMock = new();
    private readonly Mock<ILimitedFeatureRepository> _limitedFeatureRepoMock = new();
    private readonly AddonAddedDomainEventHandler _handler;

    public AddonAddedDomainEventHandlerTests()
    {
        _handler = new AddonAddedDomainEventHandler(
            _subscriptionRepoMock.Object,
            _organizationRepoMock.Object,
            _limitedFeatureAddonRepoMock.Object,
            _featureUsageRepoMock.Object,
            _planLimitedFeatureRepoMock.Object,
            _limitedFeatureRepoMock.Object);
    }

    // ===== Handle =====

    /// <summary>
    /// Verifies that Handle throws NotFoundException when the subscription is not found.
    /// </summary>
    [TestMethod]
    public async Task Handle_SubscriptionNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var notification = new AddonAddedDomainEvent(Guid.NewGuid(), new List<Guid> { Guid.NewGuid() });

        _subscriptionRepoMock
            .Setup(r => r.GetByIdWithAddonAsync(notification.SubscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(notification, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that Handle throws NotFoundException when the addons are not found.
    /// </summary>
    [TestMethod]
    public async Task Handle_AddonsNotFound_ThrowsNotFoundException()
    {
        // Arrange
        Guid planId = Guid.NewGuid();
        Guid organizationId = Guid.NewGuid();
        var subscription = CreateTestSubscription(planId, organizationId);
        var addonId = Guid.NewGuid();

        var notification = new AddonAddedDomainEvent(subscription.Id, new List<Guid> { addonId });

        _subscriptionRepoMock
            .Setup(r => r.GetByIdWithAddonAsync(subscription.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        _limitedFeatureAddonRepoMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<LimitedFeatureAddon>?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(notification, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that Handle throws NotFoundException when the organization is not found.
    /// </summary>
    [TestMethod]
    public async Task Handle_OrganizationNotFound_ThrowsNotFoundException()
    {
        // Arrange
        Guid planId = Guid.NewGuid();
        Guid organizationId = Guid.NewGuid();
        var subscription = CreateTestSubscription(planId, organizationId);
        var addonId = Guid.NewGuid();
        var addon = LimitedFeatureAddon.Create(addonId, Guid.NewGuid(), AddonType.OneTime, 10, new Money(5m, Currency.Usd));

        var notification = new AddonAddedDomainEvent(subscription.Id, new List<Guid> { addonId });

        _subscriptionRepoMock
            .Setup(r => r.GetByIdWithAddonAsync(subscription.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        _limitedFeatureAddonRepoMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LimitedFeatureAddon> { addon });

        _organizationRepoMock
            .Setup(r => r.GetByIdAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organization?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(notification, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that Handle throws NotFoundException when feature usages are not found.
    /// </summary>
    [TestMethod]
    public async Task Handle_FeatureUsageNotFound_ThrowsNotFoundException()
    {
        // Arrange
        Guid planId = Guid.NewGuid();
        Guid organizationId = Guid.NewGuid();
        var subscription = CreateTestSubscription(planId, organizationId);
        var addonId = Guid.NewGuid();
        var limitedFeatureId = Guid.NewGuid();
        var addon = LimitedFeatureAddon.Create(addonId, limitedFeatureId, AddonType.OneTime, 10, new Money(5m, Currency.Usd));
        var organization = CreateTestOrganization();

        var notification = new AddonAddedDomainEvent(subscription.Id, new List<Guid> { addonId });

        _subscriptionRepoMock
            .Setup(r => r.GetByIdWithAddonAsync(subscription.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        _limitedFeatureAddonRepoMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LimitedFeatureAddon> { addon });

        _organizationRepoMock
            .Setup(r => r.GetByIdAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organization);

        _featureUsageRepoMock
            .Setup(r => r.GetByOrganizationIdAsync(organization.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<FeatureUsage>?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(notification, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that Handle returns early without processing when all addons are already attached (idempotency).
    /// </summary>
    [TestMethod]
    public async Task Handle_AllAddonsAlreadyAttached_ReturnsNoOp()
    {
        // Arrange
        Guid planId = Guid.NewGuid();
        Guid organizationId = Guid.NewGuid();
        var subscription = CreateTestSubscription(planId, organizationId);
        var addonId = Guid.NewGuid();
        subscription.AddAddons(new List<Guid> { addonId });

        var notification = new AddonAddedDomainEvent(subscription.Id, new List<Guid> { addonId });

        _subscriptionRepoMock
            .Setup(r => r.GetByIdWithAddonAsync(subscription.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _limitedFeatureAddonRepoMock.Verify(
            r => r.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that Handle adds addon to an existing feature usage when a matching FeatureUsage exists.
    /// </summary>
    [TestMethod]
    public async Task Handle_NewAddons_AddsToExistingFeatureUsage()
    {
        // Arrange
        Guid planId = Guid.NewGuid();
        Guid organizationId = Guid.NewGuid();
        var subscription = CreateTestSubscription(planId, organizationId);
        var addonId = Guid.NewGuid();
        var limitedFeatureId = Guid.NewGuid();
        var addon = LimitedFeatureAddon.Create(addonId, limitedFeatureId, AddonType.OneTime, 15, new Money(5m, Currency.Usd));
        var organization = CreateTestOrganization();
        var limitedFeature = LimitedFeature.Branches;
        var planLimitedFeature = PlanLimitedFeature.Create(Guid.NewGuid(), planId, limitedFeatureId, 100);

        var featureUsage = FeatureUsage.Create(organization.Id, limitedFeatureId,
            subscription.SubscriptionsStartTime, subscription.SubscriptionsEndTime, 50).Value;

        var notification = new AddonAddedDomainEvent(subscription.Id, new List<Guid> { addonId });

        _subscriptionRepoMock
            .Setup(r => r.GetByIdWithAddonAsync(subscription.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        _limitedFeatureAddonRepoMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LimitedFeatureAddon> { addon });

        _organizationRepoMock
            .Setup(r => r.GetByIdAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organization);

        _featureUsageRepoMock
            .Setup(r => r.GetByOrganizationIdAsync(organization.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FeatureUsage> { featureUsage });

        _limitedFeatureRepoMock
            .Setup(r => r.GetByIdAsync(limitedFeatureId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(limitedFeature);

        _planLimitedFeatureRepoMock
            .Setup(r => r.GetPlanLimitedFeatureByLimitedFeatureIdAsync(planId, limitedFeatureId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(planLimitedFeature);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        Assert.AreEqual(65, featureUsage.Quota);
        _featureUsageRepoMock.Verify(
            r => r.Add(It.IsAny<FeatureUsage>()), Times.Never);
    }

    /// <summary>
    /// Verifies that Handle creates a new FeatureUsage when no existing FeatureUsage matches the addon's LimitedFeatureId.
    /// </summary>
    [TestMethod]
    public async Task Handle_NewAddons_CreatesNewFeatureUsage()
    {
        // Arrange
        Guid planId = Guid.NewGuid();
        Guid organizationId = Guid.NewGuid();
        var subscription = CreateTestSubscription(planId, organizationId);
        var addonId = Guid.NewGuid();
        var limitedFeatureId = Guid.NewGuid();
        var addon = LimitedFeatureAddon.Create(addonId, limitedFeatureId, AddonType.OneTime, 25, new Money(10m, Currency.Usd));
        var organization = CreateTestOrganization();
        var limitedFeature = LimitedFeature.Branches;
        var planLimitedFeature = PlanLimitedFeature.Create(Guid.NewGuid(), planId, limitedFeatureId, 200);

        var notification = new AddonAddedDomainEvent(subscription.Id, new List<Guid> { addonId });

        _subscriptionRepoMock
            .Setup(r => r.GetByIdWithAddonAsync(subscription.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        _limitedFeatureAddonRepoMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LimitedFeatureAddon> { addon });

        _organizationRepoMock
            .Setup(r => r.GetByIdAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organization);

        _featureUsageRepoMock
            .Setup(r => r.GetByOrganizationIdAsync(organization.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FeatureUsage>
            {
                FeatureUsage.Create(organization.Id, Guid.NewGuid(), subscription.SubscriptionsStartTime, subscription.SubscriptionsEndTime, 50).Value
            });

        _limitedFeatureRepoMock
            .Setup(r => r.GetByIdAsync(limitedFeatureId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(limitedFeature);

        _planLimitedFeatureRepoMock
            .Setup(r => r.GetPlanLimitedFeatureByLimitedFeatureIdAsync(planId, limitedFeatureId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(planLimitedFeature);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _featureUsageRepoMock.Verify(
            r => r.Add(It.Is<FeatureUsage>(fu =>
                fu.OrganizationId == organization.Id &&
                fu.LimitedFeatureId == limitedFeature.Id &&
                fu.Quota == planLimitedFeature.LimitValue)),
            Times.Once);
    }

    // ===== Helpers =====

    private static Subscription CreateTestSubscription(Guid planId, Guid organizationId)
    {
        var result = Subscription.Create(planId, organizationId, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));
        return result.Value;
    }

    private static Organization CreateTestOrganization()
    {
        var result = Organization.Create(
            ownerId: Guid.NewGuid(),
            countryId: Guid.NewGuid(),
            name: "Test Organization",
            about: "A test organization",
            serviceDescription: "Test services",
            serviceTypes: new List<ServiceType> { ServiceType.Cardiology },
            joinedOnUtc: DateTime.UtcNow,
            referralSource: ReferralSource.Website,
            billingEmail: "billing@test.com",
            supportEmail: "support@test.com");
        return result.Value;
    }
}
