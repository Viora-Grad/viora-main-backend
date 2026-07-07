using Moq;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Subscriptions.RenewSubscriptions;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Organizations.Shared.Enums;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Internal;
using Viora.Domain.Plans.Features;
using Viora.Domain.Shared;
using Viora.Domain.Subscriptions;
using Viora.Domain.Subscriptions.Addons;
using Viora.Domain.Subscriptions.Addons.Internal;
using Viora.Domain.Subscriptions.Events;

namespace Viora.Test.Compenents.Application.Subscriptions;

/// <summary>
/// Unit tests for the SubscriptionRenewedDomainEventHandler covering not-found scenarios and renewal with/without addons.
/// </summary>
[TestClass]
public sealed class SubscriptionRenewedDomainEventHandlerTests
{
    private readonly Mock<IFeatureUsageRepository> _featureUsageRepoMock = new();
    private readonly Mock<ISubscriptionRepository> _subscriptionRepoMock = new();
    private readonly Mock<IPlanRepository> _planRepoMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly Mock<IOrganizationRepository> _organizationRepoMock = new();
    private readonly Mock<ILimitedFeatureAddonRepository> _limitedFeatureAddonRepoMock = new();
    private readonly SubscriptionRenewedDomainEventHandler _handler;

    private static readonly DateTime UtcNow = new(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);

    public SubscriptionRenewedDomainEventHandlerTests()
    {
        _handler = new SubscriptionRenewedDomainEventHandler(
            _featureUsageRepoMock.Object,
            _subscriptionRepoMock.Object,
            _planRepoMock.Object,
            _dateTimeProviderMock.Object,
            _organizationRepoMock.Object,
            _limitedFeatureAddonRepoMock.Object);
    }

    // ===== Handle =====

    /// <summary>
    /// Verifies that Handle throws NotFoundException when the subscription is not found.
    /// </summary>
    [TestMethod]
    public async Task Handle_SubscriptionNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        _subscriptionRepoMock.Setup(r => r.GetByIdWithAddonAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        var notification = new SubscriptionRenewedDomainEvent(subscriptionId, planId, orgId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(notification, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that Handle throws NotFoundException when the plan is not found.
    /// </summary>
    [TestMethod]
    public async Task Handle_PlanNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var subscription = Subscription.Create(planId, orgId, UtcNow, UtcNow.AddMonths(1)).Value;

        _subscriptionRepoMock.Setup(r => r.GetByIdWithAddonAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);
        _planRepoMock.Setup(r => r.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Plan?)null);

        var notification = new SubscriptionRenewedDomainEvent(subscriptionId, planId, orgId);

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
        var subscriptionId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var subscription = Subscription.Create(planId, orgId, UtcNow, UtcNow.AddMonths(1)).Value;
        var plan = CreateTestPlan(planId);

        _subscriptionRepoMock.Setup(r => r.GetByIdWithAddonAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);
        _planRepoMock.Setup(r => r.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);
        _organizationRepoMock.Setup(r => r.GetByIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organization?)null);

        var notification = new SubscriptionRenewedDomainEvent(subscriptionId, planId, orgId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(notification, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that Handle renews the subscription with addons and updates feature usages with addon restore values added to base limits.
    /// </summary>
    [TestMethod]
    public async Task Handle_RenewWithAddons_UpdatesFeatureUsagesWithAddonValues()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var limitedFeatureId = LimitedFeature.StaffMembers.Id;
        var addonId = Guid.NewGuid();
        var planLimitedFeatureId = Guid.NewGuid();

        var organization = CreateTestOrganization(ownerId);
        var actualOrgId = organization.Id;

        var subscription = Subscription.Create(planId, actualOrgId, UtcNow, UtcNow.AddMonths(1)).Value;
        subscription.AddAddons(new List<Guid> { addonId });

        var plan = CreateTestPlan(planId);
        var plf = PlanLimitedFeature.Create(planLimitedFeatureId, planId, limitedFeatureId, 10);
        SetPlanLimitedFeatures(plan, new List<PlanLimitedFeature> { plf });
        SetLimitedFeatureNavigation(plf, LimitedFeature.StaffMembers);

        var limitedFeatureAddon = LimitedFeatureAddon.Create(addonId, limitedFeatureId, AddonType.TimeBase, 5, new Money(10m, Currency.Usd));

        var featureUsage = CreateTestFeatureUsage(actualOrgId, limitedFeatureId);

        _dateTimeProviderMock.SetupGet(p => p.UtcNow).Returns(UtcNow);

        _subscriptionRepoMock.Setup(r => r.GetByIdWithAddonAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        _planRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        _organizationRepoMock.Setup(r => r.GetByIdAsync(actualOrgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organization);

        _limitedFeatureAddonRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LimitedFeatureAddon> { limitedFeatureAddon });

        _featureUsageRepoMock.Setup(r => r.GetByOrganizationIdAndFeatureIdAsync(actualOrgId, limitedFeatureId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(featureUsage);

        var notification = new SubscriptionRenewedDomainEvent(subscriptionId, planId, actualOrgId);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _subscriptionRepoMock.Verify(r => r.Add(It.IsAny<Subscription>()), Times.Once);
        Assert.AreEqual(15, featureUsage.Quota);
    }

    /// <summary>
    /// Verifies that Handle renews the subscription without addons and updates feature usages with base plan limits only.
    /// </summary>
    [TestMethod]
    public async Task Handle_RenewWithoutAddons_UpdatesFeatureUsagesWithBaseLimits()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var limitedFeatureId = LimitedFeature.Branches.Id;
        var planLimitedFeatureId = Guid.NewGuid();

        var organization = CreateTestOrganization(ownerId);
        var actualOrgId = organization.Id;

        var subscription = Subscription.Create(planId, actualOrgId, UtcNow, UtcNow.AddMonths(1)).Value;

        var plan = CreateTestPlan(planId);
        var plf = PlanLimitedFeature.Create(planLimitedFeatureId, planId, limitedFeatureId, 5);
        SetPlanLimitedFeatures(plan, new List<PlanLimitedFeature> { plf });
        SetLimitedFeatureNavigation(plf, LimitedFeature.Branches);

        var featureUsage = CreateTestFeatureUsage(actualOrgId, limitedFeatureId);

        _dateTimeProviderMock.SetupGet(p => p.UtcNow).Returns(UtcNow);

        _subscriptionRepoMock.Setup(r => r.GetByIdWithAddonAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        _planRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        _organizationRepoMock.Setup(r => r.GetByIdAsync(actualOrgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organization);

        _limitedFeatureAddonRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LimitedFeatureAddon>());

        _featureUsageRepoMock.Setup(r => r.GetByOrganizationIdAndFeatureIdAsync(actualOrgId, limitedFeatureId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(featureUsage);

        var notification = new SubscriptionRenewedDomainEvent(subscriptionId, planId, actualOrgId);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _subscriptionRepoMock.Verify(r => r.Add(It.IsAny<Subscription>()), Times.Once);
        Assert.AreEqual(5, featureUsage.Quota);
    }

    // ===== Helpers =====

    private static Plan CreateTestPlan(Guid? id = null)
    {
        return Plan.Create(
            id ?? Guid.NewGuid(),
            "Test Plan",
            "Description",
            "Content",
            99.99m,
            Currency.Usd,
            PlanPeriod.monthly);
    }

    private static Organization CreateTestOrganization(Guid id)
    {
        return Organization.Create(
            id,
            Guid.NewGuid(),
            "TestOrg",
            "Test about",
            "Test service description",
            new List<ServiceType> { ServiceType.InternalMedicine },
            DateTime.UtcNow,
            ReferralSource.Friend,
            "test@email.com",
            "support@email.com").Value;
    }

    private static FeatureUsage CreateTestFeatureUsage(Guid orgId, Guid limitedFeatureId)
    {
        return FeatureUsage.Create(orgId, limitedFeatureId, UtcNow.AddMonths(-1), UtcNow, 0).Value;
    }

    private static void SetPlanLimitedFeatures(Plan plan, List<PlanLimitedFeature> features)
    {
        typeof(Plan)
            .GetProperty(nameof(Plan.PlanLimitedFeatures))!
            .SetValue(plan, features);
    }

    private static void SetLimitedFeatureNavigation(PlanLimitedFeature plf, LimitedFeature limitedFeature)
    {
        typeof(PlanLimitedFeature)
            .GetProperty(nameof(PlanLimitedFeature.LimitedFeature))!
            .SetValue(plf, limitedFeature);
    }
}
