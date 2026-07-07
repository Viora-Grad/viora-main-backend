using Moq;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Orders.CreateAddonOrder;
using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Organizations.Shared.Enums;
using Viora.Domain.Orders;
using Viora.Domain.Shared;
using Viora.Domain.Subscriptions.Addons;
using Viora.Domain.Subscriptions.Addons.Internal;
using Viora.Domain.Subscriptions;
using Subscription = Viora.Domain.Subscriptions.Subscription;

namespace Viora.Test.Compenents.Application.Orders;

/// <summary>
/// Unit tests for the CreateAddonOrderCommandHandler covering successful creation and not-found scenarios.
/// </summary>
[TestClass]
public sealed class CreateAddonOrderCommandHandlerTests
{
    private readonly Mock<IOrganizationRepository> _organizationRepoMock = new();
    private readonly Mock<ISubscriptionRepository> _subscriptionRepoMock = new();
    private readonly Mock<ILimitedFeatureAddonRepository> _limitedFeatureAddonRepoMock = new();
    private readonly Mock<IAddonOrderRepository> _addonOrderRepoMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly CreateAddonOrderCommandHandler _handler;

    public CreateAddonOrderCommandHandlerTests()
    {
        _handler = new CreateAddonOrderCommandHandler(
            _organizationRepoMock.Object,
            _subscriptionRepoMock.Object,
            _limitedFeatureAddonRepoMock.Object,
            _addonOrderRepoMock.Object,
            _dateTimeProviderMock.Object,
            _unitOfWorkMock.Object);
    }

    // ===== Handle =====

    /// <summary>
    /// Verifies that Handle with valid inputs creates an addon order and returns its ID.
    /// </summary>
    [TestMethod]
    public async Task Handle_ValidInputs_CreatesAddonOrderAndReturnsId()
    {
        // Arrange
        var org = CreateTestOrganization();
        Guid orgId = org.Id;
        Guid subscriptionId = Guid.NewGuid();
        Guid addonId = Guid.NewGuid();
        var subscription = CreateTestSubscription(orgId);
        var addons = new List<LimitedFeatureAddon>
        {
            LimitedFeatureAddon.Create(addonId, Guid.NewGuid(), AddonType.OneTime, 10, new Money(5m, Currency.Usd))
        };

        _organizationRepoMock.Setup(r => r.GetByIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _subscriptionRepoMock.Setup(r => r.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);
        _limitedFeatureAddonRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(addons);
        _dateTimeProviderMock.SetupGet(p => p.UtcNow).Returns(DateTime.UtcNow);

        // Act
        Result<Guid> result = await _handler.Handle(
            new CreateAddonOrderCommand(orgId, subscriptionId, new List<Guid> { addonId }), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreNotEqual(Guid.Empty, result.Value);
        _addonOrderRepoMock.Verify(r => r.Add(It.IsAny<AddonOrder>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that Handle with non-existent organization throws NotFoundException.
    /// </summary>
    [TestMethod]
    public async Task Handle_OrganizationNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _organizationRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organization?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new CreateAddonOrderCommand(Guid.NewGuid(), Guid.NewGuid(), new List<Guid> { Guid.NewGuid() }), CancellationToken.None));
    }

    /// <summary>
    /// Verifies that Handle with non-existent subscription throws NotFoundException.
    /// </summary>
    [TestMethod]
    public async Task Handle_SubscriptionNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var org = CreateTestOrganization();
        _organizationRepoMock.Setup(r => r.GetByIdAsync(org.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _subscriptionRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new CreateAddonOrderCommand(org.Id, Guid.NewGuid(), new List<Guid> { Guid.NewGuid() }), CancellationToken.None));
    }

    /// <summary>
    /// Verifies that Handle with subscription belonging to different organization returns failure.
    /// </summary>
    [TestMethod]
    public async Task Handle_SubscriptionDifferentOrganization_ReturnsFailure()
    {
        // Arrange
        var org = CreateTestOrganization();
        Guid subscriptionId = Guid.NewGuid();
        var subscription = CreateTestSubscription(Guid.NewGuid());

        _organizationRepoMock.Setup(r => r.GetByIdAsync(org.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _subscriptionRepoMock.Setup(r => r.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        // Act
        Result<Guid> result = await _handler.Handle(
            new CreateAddonOrderCommand(org.Id, subscriptionId, new List<Guid> { Guid.NewGuid() }), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailure);
    }

    /// <summary>
    /// Verifies that Handle with non-existent addons throws NotFoundException.
    /// </summary>
    [TestMethod]
    public async Task Handle_AddonsNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var org = CreateTestOrganization();
        var subscription = CreateTestSubscription(org.Id);
        _organizationRepoMock.Setup(r => r.GetByIdAsync(org.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _subscriptionRepoMock.Setup(r => r.GetByIdAsync(subscription.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);
        _limitedFeatureAddonRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LimitedFeatureAddon>());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new CreateAddonOrderCommand(org.Id, subscription.Id, new List<Guid> { Guid.NewGuid() }), CancellationToken.None));
    }

    // ===== Helpers =====

    private static Organization CreateTestOrganization()
    {
        return Organization.Create(Guid.NewGuid(), Guid.NewGuid(), "TestOrg", "Test about", "Test service description", new List<ServiceType> { ServiceType.InternalMedicine }, DateTime.UtcNow, ReferralSource.Friend, "test@example.com", "support@example.com").Value;
    }

    private static Subscription CreateTestSubscription(Guid orgId)
    {
        return Subscription.Create(Guid.NewGuid(), orgId, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1)).Value;
    }
}
