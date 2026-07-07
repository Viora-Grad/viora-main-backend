using Moq;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Subscriptions.CreateSubscriptions;
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
/// Unit tests for the SubscriptionCreatedDomainEventHandler covering idempotency, happy path, and not-found scenarios.
/// </summary>
[TestClass]
public sealed class SubscriptionCreatedDomainEventHandlerTests
{
    private readonly Mock<IPlanRepository> _planRepoMock = new();
    private readonly Mock<IOrganizationRepository> _organizationRepoMock = new();
    private readonly Mock<ISubscriptionRepository> _subscriptionRepoMock = new();
    private readonly Mock<IFeatureUsageRepository> _featureUsageRepoMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly SubscriptionCreatedDomainEventHandler _handler;

    public SubscriptionCreatedDomainEventHandlerTests()
    {
        _handler = new SubscriptionCreatedDomainEventHandler(
            _planRepoMock.Object,
            _organizationRepoMock.Object,
            _subscriptionRepoMock.Object,
            _featureUsageRepoMock.Object,
            _dateTimeProviderMock.Object);
    }

    // ===== Handle =====

    /// <summary>
    /// Verifies that Handle when the organization already has a subscription returns without further processing.
    /// </summary>
    [TestMethod]
    public async Task Handle_OrganizationAlreadyHasSubscription_NoOp()
    {
        // Arrange
        Guid orgId = Guid.NewGuid();
        Guid planId = Guid.NewGuid();
        var existingSubscription = CreateTestSubscription(planId, orgId);

        _subscriptionRepoMock.Setup(r => r.GetByOrganizationIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSubscription);

        var notification = new SubscriptionCreatedDomainEvent(planId, orgId);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _planRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _organizationRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _subscriptionRepoMock.Verify(r => r.Add(It.IsAny<Subscription>()), Times.Never);
        _featureUsageRepoMock.Verify(r => r.Add(It.IsAny<FeatureUsage>()), Times.Never);
    }

    /// <summary>
    /// Verifies that Handle with valid inputs creates a subscription and feature usages for each plan limited feature.
    /// </summary>
    [TestMethod]
    public async Task Handle_NewSubscription_CreatesSubscriptionAndFeatureUsages()
    {
        // Arrange
        Guid orgId = Guid.NewGuid();
        Guid planId = Guid.NewGuid();
        DateTime now = new(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        DateTime expectedEnd = now.AddMonths(1);

        var plan = CreateTestPlan(planId);
        var organization = CreateTestOrganization(orgId);
        SetPlanLimitedFeatures(plan, new List<PlanLimitedFeature>
        {
            PlanLimitedFeature.Create(Guid.NewGuid(), planId, LimitedFeature.Branches.Id, 5),
            PlanLimitedFeature.Create(Guid.NewGuid(), planId, LimitedFeature.StaffMembers.Id, 10)
        });

        _subscriptionRepoMock.Setup(r => r.GetByOrganizationIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);
        _planRepoMock.Setup(r => r.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);
        _organizationRepoMock.Setup(r => r.GetByIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(organization);
        _dateTimeProviderMock.SetupGet(p => p.UtcNow).Returns(now);

        var notification = new SubscriptionCreatedDomainEvent(planId, orgId);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _subscriptionRepoMock.Verify(r => r.Add(It.IsAny<Subscription>()), Times.Once);
        _featureUsageRepoMock.Verify(r => r.Add(It.IsAny<FeatureUsage>()), Times.Exactly(2));
    }

    /// <summary>
    /// Verifies that Handle when plan is not found throws NotFoundException.
    /// </summary>
    [TestMethod]
    public async Task Handle_PlanNotFound_ThrowsNotFoundException()
    {
        // Arrange
        Guid orgId = Guid.NewGuid();
        Guid planId = Guid.NewGuid();

        _subscriptionRepoMock.Setup(r => r.GetByOrganizationIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);
        _planRepoMock.Setup(r => r.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Plan?)null);

        var notification = new SubscriptionCreatedDomainEvent(planId, orgId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(notification, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that Handle when organization is not found throws NotFoundException.
    /// </summary>
    [TestMethod]
    public async Task Handle_OrganizationNotFound_ThrowsNotFoundException()
    {
        // Arrange
        Guid orgId = Guid.NewGuid();
        Guid planId = Guid.NewGuid();

        _subscriptionRepoMock.Setup(r => r.GetByOrganizationIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);
        _planRepoMock.Setup(r => r.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestPlan(planId));
        _organizationRepoMock.Setup(r => r.GetByIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organization?)null);

        var notification = new SubscriptionCreatedDomainEvent(planId, orgId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(notification, CancellationToken.None));
    }

    // ===== Helpers =====

    private static Subscription CreateTestSubscription(Guid planId, Guid orgId)
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

    private static void SetPlanLimitedFeatures(Plan plan, List<PlanLimitedFeature> features)
    {
        var property = typeof(Plan).GetProperty(nameof(Plan.PlanLimitedFeatures))!;
        property.SetValue(plan, features);
    }
}
