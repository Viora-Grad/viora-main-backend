using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Subscriptions.ChangeSubscription;
using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Organizations.Shared.Enums;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Features;
using Viora.Domain.Plans.Internal;
using Viora.Domain.Shared;
using Viora.Domain.Subscriptions;
using Viora.Domain.Subscriptions.Events;

namespace Viora.Test.Compenents.Application.Subscriptions;

/// <summary>
/// Unit tests for the SubscriptionPlanChangeDomainEventHandler covering plan change scenarios.
/// </summary>
[TestClass]
public sealed class SubscriptionPlanChangeDomainEventHandlerTests
{
    private readonly Mock<IPlanRepository> _planRepoMock = new();
    private readonly Mock<IFeatureUsageRepository> _featureUsageRepoMock = new();
    private readonly Mock<ISubscriptionRepository> _subscriptionRepoMock = new();
    private readonly Mock<IOrganizationRepository> _organizationRepoMock = new();
    private readonly Mock<ILogger<SubscriptionPlanChangeDomainEventHandler>> _loggerMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly SubscriptionPlanChangeDomainEventHandler _handler;

    private static readonly DateTime UtcNow = new(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);

    public SubscriptionPlanChangeDomainEventHandlerTests()
    {
        _handler = new SubscriptionPlanChangeDomainEventHandler(
            _planRepoMock.Object,
            _featureUsageRepoMock.Object,
            _subscriptionRepoMock.Object,
            _organizationRepoMock.Object,
            _loggerMock.Object,
            _dateTimeProviderMock.Object);
    }

    // ===== Handle =====

    /// <summary>
    /// Verifies that Handle logs a warning and returns when the organization no longer exists.
    /// </summary>
    [TestMethod]
    public async Task Handle_OrganizationNotFound_ReturnsNoOp()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var oldPlanId = Guid.NewGuid();
        var newPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var newPlan = CreateTestPlan(newPlanId);
        var subscription = CreateTestSubscription(subscriptionId, oldPlanId, orgId);

        _planRepoMock.Setup(r => r.GetByIdAsync(newPlanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newPlan);
        _subscriptionRepoMock.Setup(r => r.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);
        _organizationRepoMock.Setup(r => r.ExistsAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var notification = new SubscriptionPlanChangedDomainEvent(subscriptionId, oldPlanId, newPlanId, orgId, UtcNow, UtcNow.AddMonths(1));

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _subscriptionRepoMock.Verify(r => r.Add(It.IsAny<Subscription>()), Times.Never);
        _featureUsageRepoMock.Verify(r => r.RemoveRangeByLimitedIdAndOrganizationId(It.IsAny<IEnumerable<Guid>>(), It.IsAny<Guid>()), Times.Never);
    }

    /// <summary>
    /// Verifies that Handle throws NotFoundException when the new plan does not exist.
    /// </summary>
    [TestMethod]
    public async Task Handle_NewPlanNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var oldPlanId = Guid.NewGuid();
        var newPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        _planRepoMock.Setup(r => r.GetByIdAsync(newPlanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Plan?)null);

        var notification = new SubscriptionPlanChangedDomainEvent(subscriptionId, oldPlanId, newPlanId, orgId, UtcNow, UtcNow.AddMonths(1));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(notification, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that Handle throws NotFoundException when the subscription does not exist.
    /// </summary>
    [TestMethod]
    public async Task Handle_SubscriptionNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var oldPlanId = Guid.NewGuid();
        var newPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var newPlan = CreateTestPlan(newPlanId);

        _planRepoMock.Setup(r => r.GetByIdAsync(newPlanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newPlan);
        _subscriptionRepoMock.Setup(r => r.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        var notification = new SubscriptionPlanChangedDomainEvent(subscriptionId, oldPlanId, newPlanId, orgId, UtcNow, UtcNow.AddMonths(1));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(notification, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that Handle with valid inputs creates a new subscription, updates feature usages, and saves.
    /// </summary>
    [TestMethod]
    public async Task Handle_ValidChange_CreatesNewSubscriptionAndUpdatesFeatures()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var oldPlanId = Guid.NewGuid();
        var newPlanId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var oldPlan = CreateTestPlan(oldPlanId);
        var newPlan = CreateTestPlan(newPlanId);

        var limitedFeatureId1 = LimitedFeature.StaffMembers.Id;
        var limitedFeatureId2 = LimitedFeature.Branches.Id;

        var plf1 = PlanLimitedFeature.Create(Guid.NewGuid(), newPlanId, limitedFeatureId1, 10);
        var plf2 = PlanLimitedFeature.Create(Guid.NewGuid(), newPlanId, limitedFeatureId2, 5);
        SetPlanLimitedFeatures(newPlan, new List<PlanLimitedFeature> { plf1, plf2 });

        var oldPlf1 = PlanLimitedFeature.Create(Guid.NewGuid(), oldPlanId, limitedFeatureId1, 5);
        SetPlanLimitedFeatures(oldPlan, new List<PlanLimitedFeature> { oldPlf1 });

        var subscription = CreateTestSubscription(subscriptionId, oldPlanId, orgId);
        var endTime = UtcNow.AddMonths(1);

        _dateTimeProviderMock.SetupGet(p => p.UtcNow).Returns(UtcNow);

        _planRepoMock.Setup(r => r.GetByIdAsync(newPlanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newPlan);
        _subscriptionRepoMock.Setup(r => r.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);
        _organizationRepoMock.Setup(r => r.ExistsAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _planRepoMock.Setup(r => r.GetByIdAsync(oldPlanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldPlan);

        var notification = new SubscriptionPlanChangedDomainEvent(subscriptionId, oldPlanId, newPlanId, orgId, UtcNow, endTime);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _subscriptionRepoMock.Verify(r => r.Add(It.IsAny<Subscription>()), Times.Once);
        _featureUsageRepoMock.Verify(r => r.RemoveRangeByLimitedIdAndOrganizationId(
            It.Is<IEnumerable<Guid>>(ids => ids.Contains(limitedFeatureId1)),
            orgId), Times.Once);
        _featureUsageRepoMock.Verify(r => r.Add(It.IsAny<FeatureUsage>()), Times.Exactly(2));
    }

    // ===== Helpers =====

    private static Subscription CreateTestSubscription(Guid id, Guid planId, Guid orgId)
    {
        return Subscription.Create(planId, orgId, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1)).Value;
    }

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

    private static void SetPlanLimitedFeatures(Plan plan, List<PlanLimitedFeature> features)
    {
        typeof(Plan)
            .GetProperty(nameof(Plan.PlanLimitedFeatures))!
            .SetValue(plan, features);
    }
}
