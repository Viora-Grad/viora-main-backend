using Moq;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Subscriptions.RemoveAddon;
using Viora.Domain.Abstractions;
using Viora.Domain.Shared;
using Viora.Domain.Subscriptions;
using Viora.Domain.Subscriptions.Addons;
using Viora.Domain.Subscriptions.Addons.Internal;

namespace Viora.Test.Compenents.Application.Subscriptions;

/// <summary>
/// Unit tests for the RemoveAddonCommandHandler covering successful removal and not-found scenarios.
/// </summary>
[TestClass]
public sealed class RemoveAddonCommandHandlerTests
{
    private readonly Mock<ISubscriptionRepository> _subscriptionRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly RemoveAddonCommandHandler _handler;

    public RemoveAddonCommandHandlerTests()
    {
        _handler = new RemoveAddonCommandHandler(
            _subscriptionRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    // ===== Handle =====

    /// <summary>
    /// Verifies that Handle with valid subscription and addon IDs removes the addon and saves.
    /// </summary>
    [TestMethod]
    public async Task Handle_ValidIds_RemovesAddonAndSaves()
    {
        // Arrange
        Guid subscriptionId = Guid.NewGuid();
        Guid addonId = Guid.NewGuid();
        var subscription = CreateTestSubscription(subscriptionId);
        var limitedFeatureAddon = LimitedFeatureAddon.Create(addonId, Guid.NewGuid(), AddonType.OneTime, 10, new Money(5m, Currency.Usd));
        subscription.AddAddons(new List<Guid> { addonId });

        var addon = subscription.GetAddons().First();

        _subscriptionRepoMock.Setup(r => r.GetByIdWithAddonAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        var command = new RemoveAddonCommand(subscriptionId, addon.Id);

        // Act
        Result result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that Handle with non-existent subscription throws NotFoundException.
    /// </summary>
    [TestMethod]
    public async Task Handle_SubscriptionNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var command = new RemoveAddonCommand(Guid.NewGuid(), Guid.NewGuid());

        _subscriptionRepoMock.Setup(r => r.GetByIdWithAddonAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that Handle with non-existent addon ID on subscription throws NotFoundException.
    /// </summary>
    [TestMethod]
    public async Task Handle_AddonNotFound_ThrowsNotFoundException()
    {
        // Arrange
        Guid subscriptionId = Guid.NewGuid();
        var subscription = CreateTestSubscription(subscriptionId);

        _subscriptionRepoMock.Setup(r => r.GetByIdWithAddonAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        var command = new RemoveAddonCommand(subscriptionId, Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that Handle passes the cancellation token to the repository.
    /// </summary>
    [TestMethod]
    public async Task Handle_CallsRepositoryWithCancellationToken()
    {
        // Arrange
        Guid subscriptionId = Guid.NewGuid();
        var subscription = CreateTestSubscription(subscriptionId);
        subscription.AddAddons(new List<Guid> { Guid.NewGuid() });
        var addon = subscription.GetAddons().First();
        var cts = new CancellationTokenSource();

        _subscriptionRepoMock.Setup(r => r.GetByIdWithAddonAsync(subscriptionId, cts.Token))
            .ReturnsAsync(subscription);

        // Act
        await _handler.Handle(new RemoveAddonCommand(subscriptionId, addon.Id), cts.Token);

        // Assert
        _subscriptionRepoMock.Verify(r => r.GetByIdWithAddonAsync(subscriptionId, cts.Token), Times.Once);
    }

    // ===== Helpers =====

    private static Subscription CreateTestSubscription(Guid id)
    {
        var result = Subscription.Create(Guid.NewGuid(), id, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));
        return result.Value;
    }
}
