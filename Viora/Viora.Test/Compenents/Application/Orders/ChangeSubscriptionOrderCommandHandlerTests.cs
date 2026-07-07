using Moq;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Orders.ChangeSubscriptionOrder;
using Viora.Domain.Abstractions;
using Viora.Domain.Orders;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Internal;
using Viora.Domain.Shared;
using Viora.Domain.Subscriptions;
using Subscription = Viora.Domain.Subscriptions.Subscription;

namespace Viora.Test.Compenents.Application.Orders;

/// <summary>
/// Unit tests for the ChangeSubscriptionOrderCommandHandler covering successful creation, not-found, and same-plan scenarios.
/// </summary>
[TestClass]
public sealed class ChangeSubscriptionOrderCommandHandlerTests
{
    private readonly Mock<ISubscriptionRepository> _subscriptionRepoMock = new();
    private readonly Mock<IPlanRepository> _planRepoMock = new();
    private readonly Mock<ISubscriptionOrderRepository> _subscriptionOrderRepoMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ChangeSubscriptionOrderCommandHandler _handler;

    public ChangeSubscriptionOrderCommandHandlerTests()
    {
        _handler = new ChangeSubscriptionOrderCommandHandler(
            _subscriptionRepoMock.Object,
            _planRepoMock.Object,
            _subscriptionOrderRepoMock.Object,
            _dateTimeProviderMock.Object,
            _unitOfWorkMock.Object);
    }

    // ===== Handle =====

    /// <summary>
    /// Verifies that Handle with valid inputs creates a change order and returns its ID.
    /// </summary>
    [TestMethod]
    public async Task Handle_ValidInputs_CreatesChangeOrderAndReturnsId()
    {
        // Arrange
        Guid subscriptionId = Guid.NewGuid();
        Guid oldPlanId = Guid.NewGuid();
        Guid newPlanId = Guid.NewGuid();
        var subscription = CreateTestSubscription(subscriptionId, oldPlanId);
        var newPlan = CreateTestPlan(newPlanId);

        _subscriptionRepoMock.Setup(r => r.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);
        _planRepoMock.Setup(r => r.GetByIdAsync(newPlanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newPlan);
        _dateTimeProviderMock.SetupGet(p => p.UtcNow).Returns(DateTime.UtcNow);

        // Act
        Result<Guid> result = await _handler.Handle(
            new ChangeSubscriptionOrderCommand(subscriptionId, newPlanId), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreNotEqual(Guid.Empty, result.Value);
        _subscriptionOrderRepoMock.Verify(r => r.Add(It.IsAny<SubscriptionOrder>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that Handle with non-existent subscription throws NotFoundException.
    /// </summary>
    [TestMethod]
    public async Task Handle_SubscriptionNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _subscriptionRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new ChangeSubscriptionOrderCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None));
    }

    /// <summary>
    /// Verifies that Handle with non-existent new plan throws NotFoundException.
    /// </summary>
    [TestMethod]
    public async Task Handle_NewPlanNotFound_ThrowsNotFoundException()
    {
        // Arrange
        Guid subscriptionId = Guid.NewGuid();
        _subscriptionRepoMock.Setup(r => r.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestSubscription(subscriptionId));
        _planRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Plan?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new ChangeSubscriptionOrderCommand(subscriptionId, Guid.NewGuid()), CancellationToken.None));
    }

    /// <summary>
    /// Verifies that Handle with same plan ID returns failure with InvalidPlan error.
    /// </summary>
    [TestMethod]
    public async Task Handle_SamePlanId_ReturnsFailure()
    {
        // Arrange
        Guid subscriptionId = Guid.NewGuid();
        Guid planId = Guid.NewGuid();
        var subscription = CreateTestSubscription(subscriptionId, planId);
        var plan = CreateTestPlan(planId);

        _subscriptionRepoMock.Setup(r => r.GetByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);
        _planRepoMock.Setup(r => r.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        // Act
        Result<Guid> result = await _handler.Handle(
            new ChangeSubscriptionOrderCommand(subscriptionId, planId), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailure);
    }

    /// <summary>
    /// Verifies that Handle passes the cancellation token to repositories.
    /// </summary>
    [TestMethod]
    public async Task Handle_CallsRepositoriesWithCancellationToken()
    {
        // Arrange
        Guid subscriptionId = Guid.NewGuid();
        Guid newPlanId = Guid.NewGuid();
        var cts = new CancellationTokenSource();

        _subscriptionRepoMock.Setup(r => r.GetByIdAsync(subscriptionId, cts.Token))
            .ReturnsAsync(CreateTestSubscription(subscriptionId));
        _planRepoMock.Setup(r => r.GetByIdAsync(newPlanId, cts.Token))
            .ReturnsAsync(CreateTestPlan(newPlanId));
        _dateTimeProviderMock.SetupGet(p => p.UtcNow).Returns(DateTime.UtcNow);

        // Act
        await _handler.Handle(new ChangeSubscriptionOrderCommand(subscriptionId, newPlanId), cts.Token);

        // Assert
        _subscriptionRepoMock.Verify(r => r.GetByIdAsync(subscriptionId, cts.Token), Times.Once);
        _planRepoMock.Verify(r => r.GetByIdAsync(newPlanId, cts.Token), Times.Once);
    }

    // ===== Helpers =====

    private static Subscription CreateTestSubscription(Guid id, Guid? planId = null)
    {
        return Subscription.Create(
            planId ?? Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow.AddMonths(1)).Value;
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
