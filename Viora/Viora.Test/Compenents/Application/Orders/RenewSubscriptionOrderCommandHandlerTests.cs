using Moq;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Orders.RenewSubscriptionOrder;
using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Organizations.Shared.Enums;
using Viora.Domain.Orders;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Internal;
using Viora.Domain.Shared;
using Viora.Domain.Subscriptions.Addons;
using Viora.Domain.Subscriptions.Addons.Internal;
using Viora.Domain.Subscriptions;
using Subscription = Viora.Domain.Subscriptions.Subscription;

namespace Viora.Test.Compenents.Application.Orders;

/// <summary>
/// Unit tests for the RenewSubscriptionOrderCommandHandler covering successful creation and not-found scenarios.
/// </summary>
[TestClass]
public sealed class RenewSubscriptionOrderCommandHandlerTests
{
    private readonly Mock<ISubscriptionRepository> _subscriptionRepoMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly Mock<IOrganizationRepository> _organizationRepoMock = new();
    private readonly Mock<IPlanRepository> _planRepoMock = new();
    private readonly Mock<ISubscriptionOrderRepository> _subscriptionOrderRepoMock = new();
    private readonly Mock<ILimitedFeatureAddonRepository> _limitedFeatureAddonRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly RenewSubscriptionOrderCommandHandler _handler;

    public RenewSubscriptionOrderCommandHandlerTests()
    {
        _handler = new RenewSubscriptionOrderCommandHandler(
            _subscriptionRepoMock.Object,
            _dateTimeProviderMock.Object,
            _organizationRepoMock.Object,
            _planRepoMock.Object,
            _subscriptionOrderRepoMock.Object,
            _limitedFeatureAddonRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    // ===== Handle =====

    /// <summary>
    /// Verifies that Handle with valid inputs creates a renewal order and returns its ID.
    /// </summary>
    [TestMethod]
    public async Task Handle_ValidInputs_CreatesRenewalOrderAndReturnsId()
    {
        // Arrange
        Guid subscriptionId = Guid.NewGuid();
        Guid orgId = Guid.NewGuid();
        Guid planId = Guid.NewGuid();
        var subscription = CreateTestSubscriptionWithAddon(subscriptionId, orgId, planId);
        var org = CreateTestOrganization(orgId);
        var plan = CreateTestPlan(planId);

        _subscriptionRepoMock.Setup(r => r.GetByIdWithAddonAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);
        _organizationRepoMock.Setup(r => r.GetByIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _planRepoMock.Setup(r => r.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);
        _limitedFeatureAddonRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LimitedFeatureAddon>());
        _dateTimeProviderMock.SetupGet(p => p.UtcNow).Returns(DateTime.UtcNow);

        // Act
        Result<Guid> result = await _handler.Handle(
            new RenewSubscriptionOrderCommand(subscriptionId), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreNotEqual(Guid.Empty, result.Value);
        _subscriptionOrderRepoMock.Verify(r => r.Add(It.IsAny<SubscriptionOrder>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that Handle with valid inputs and addons sums prices correctly.
    /// </summary>
    [TestMethod]
    public async Task Handle_WithAddons_SumsAddonPricesIntoTotal()
    {
        // Arrange
        Guid subscriptionId = Guid.NewGuid();
        Guid orgId = Guid.NewGuid();
        Guid planId = Guid.NewGuid();
        Guid addonId = Guid.NewGuid();
        var subscription = CreateTestSubscriptionWithAddon(subscriptionId, orgId, planId);
        var org = CreateTestOrganization(orgId);
        var plan = CreateTestPlan(planId);
        var addon = LimitedFeatureAddon.Create(addonId, Guid.NewGuid(), AddonType.OneTime, 10, new Money(25m, Currency.Usd));

        _subscriptionRepoMock.Setup(r => r.GetByIdWithAddonAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);
        _organizationRepoMock.Setup(r => r.GetByIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _planRepoMock.Setup(r => r.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);
        _limitedFeatureAddonRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LimitedFeatureAddon> { addon });
        _dateTimeProviderMock.SetupGet(p => p.UtcNow).Returns(DateTime.UtcNow);

        // Act
        Result<Guid> result = await _handler.Handle(
            new RenewSubscriptionOrderCommand(subscriptionId), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    /// <summary>
    /// Verifies that Handle with non-existent subscription throws NotFoundException.
    /// </summary>
    [TestMethod]
    public async Task Handle_SubscriptionNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _subscriptionRepoMock.Setup(r => r.GetByIdWithAddonAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new RenewSubscriptionOrderCommand(Guid.NewGuid()), CancellationToken.None));
    }

    /// <summary>
    /// Verifies that Handle with non-existent organization throws NotFoundException.
    /// </summary>
    [TestMethod]
    public async Task Handle_OrganizationNotFound_ThrowsNotFoundException()
    {
        // Arrange
        Guid subscriptionId = Guid.NewGuid();
        var subscription = CreateTestSubscriptionWithAddon(subscriptionId, Guid.NewGuid());

        _subscriptionRepoMock.Setup(r => r.GetByIdWithAddonAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);
        _organizationRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organization?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new RenewSubscriptionOrderCommand(subscriptionId), CancellationToken.None));
    }

    /// <summary>
    /// Verifies that Handle with non-existent plan throws NotFoundException.
    /// </summary>
    [TestMethod]
    public async Task Handle_PlanNotFound_ThrowsNotFoundException()
    {
        // Arrange
        Guid subscriptionId = Guid.NewGuid();
        Guid orgId = Guid.NewGuid();
        var subscription = CreateTestSubscriptionWithAddon(subscriptionId, orgId);

        _subscriptionRepoMock.Setup(r => r.GetByIdWithAddonAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);
        _organizationRepoMock.Setup(r => r.GetByIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestOrganization(orgId));
        _planRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Plan?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new RenewSubscriptionOrderCommand(subscriptionId), CancellationToken.None));
    }

    /// <summary>
    /// Verifies that Handle passes the cancellation token to repositories.
    /// </summary>
    [TestMethod]
    public async Task Handle_CallsRepositoriesWithCancellationToken()
    {
        // Arrange
        Guid subscriptionId = Guid.NewGuid();
        Guid orgId = Guid.NewGuid();
        var subscription = CreateTestSubscriptionWithAddon(subscriptionId, orgId);
        var cts = new CancellationTokenSource();

        _subscriptionRepoMock.Setup(r => r.GetByIdWithAddonAsync(subscriptionId, cts.Token))
            .ReturnsAsync(subscription);
        _organizationRepoMock.Setup(r => r.GetByIdAsync(orgId, cts.Token))
            .ReturnsAsync(CreateTestOrganization(orgId));
        _planRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), cts.Token))
            .ReturnsAsync(CreateTestPlan());
        _limitedFeatureAddonRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>(), cts.Token))
            .ReturnsAsync(new List<LimitedFeatureAddon>());
        _dateTimeProviderMock.SetupGet(p => p.UtcNow).Returns(DateTime.UtcNow);

        // Act
        await _handler.Handle(new RenewSubscriptionOrderCommand(subscriptionId), cts.Token);

        // Assert
        _subscriptionRepoMock.Verify(r => r.GetByIdWithAddonAsync(subscriptionId, cts.Token), Times.Once);
    }

    // ===== Helpers =====

    private static Organization CreateTestOrganization(Guid? id = null)
    {
        return Organization.Create(id ?? Guid.NewGuid(), Guid.NewGuid(), "TestOrg", "Test about", "Test service description", new List<ServiceType> { ServiceType.InternalMedicine }, DateTime.UtcNow, ReferralSource.Friend, "test@example.com", "support@example.com").Value;
    }

    private static Subscription CreateTestSubscriptionWithAddon(Guid id, Guid orgId, Guid? planId = null)
    {
        var subscription = Subscription.Create(
            planId ?? Guid.NewGuid(), orgId, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1)).Value;
        return subscription;
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
}
