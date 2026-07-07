using Moq;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Orders.CreateSubscriptionOrder;
using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Organizations.Shared.Enums;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Internal;
using Viora.Domain.Orders;
using Viora.Domain.Shared;

namespace Viora.Test.Compenents.Application.Orders;

/// <summary>
/// Unit tests for the CreateSubscriptionOrderCommandHandler covering successful creation and not-found scenarios.
/// </summary>
[TestClass]
public sealed class CreateSubscriptionOrderCommandHandlerTests
{
    private readonly Mock<IOrganizationRepository> _organizationRepoMock = new();
    private readonly Mock<IPlanRepository> _planRepoMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly Mock<ISubscriptionOrderRepository> _orderRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly CreateSubscriptionOrderCommandHandler _handler;

    public CreateSubscriptionOrderCommandHandlerTests()
    {
        _handler = new CreateSubscriptionOrderCommandHandler(
            _organizationRepoMock.Object,
            _planRepoMock.Object,
            _dateTimeProviderMock.Object,
            _orderRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    // ===== Handle =====

    /// <summary>
    /// Verifies that Handle with valid inputs creates an order and returns its ID.
    /// </summary>
    [TestMethod]
    public async Task Handle_ValidInputs_CreatesOrderAndReturnsId()
    {
        // Arrange
        Guid orgId = Guid.NewGuid();
        Guid planId = Guid.NewGuid();
        var org = CreateTestOrganization(orgId);
        var plan = CreateTestPlan(planId);
        DateTime now = DateTime.UtcNow;

        _organizationRepoMock.Setup(r => r.GetByIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _planRepoMock.Setup(r => r.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);
        _dateTimeProviderMock.SetupGet(p => p.UtcNow).Returns(now);

        // Act
        Result<Guid> result = await _handler.Handle(
            new CreateSubscriptionOrderCommand(orgId, planId), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreNotEqual(Guid.Empty, result.Value);
        _orderRepoMock.Verify(r => r.Add(It.IsAny<SubscriptionOrder>()), Times.Once);
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
            _handler.Handle(new CreateSubscriptionOrderCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None));
    }

    /// <summary>
    /// Verifies that Handle with non-existent plan throws NotFoundException.
    /// </summary>
    [TestMethod]
    public async Task Handle_PlanNotFound_ThrowsNotFoundException()
    {
        // Arrange
        Guid orgId = Guid.NewGuid();
        _organizationRepoMock.Setup(r => r.GetByIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestOrganization(orgId));
        _planRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Plan?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new CreateSubscriptionOrderCommand(orgId, Guid.NewGuid()), CancellationToken.None));
    }

    /// <summary>
    /// Verifies that Handle passes the cancellation token to repositories.
    /// </summary>
    [TestMethod]
    public async Task Handle_CallsRepositoriesWithCancellationToken()
    {
        // Arrange
        Guid orgId = Guid.NewGuid();
        Guid planId = Guid.NewGuid();
        var cts = new CancellationTokenSource();

        _organizationRepoMock.Setup(r => r.GetByIdAsync(orgId, cts.Token))
            .ReturnsAsync(CreateTestOrganization(orgId));
        _planRepoMock.Setup(r => r.GetByIdAsync(planId, cts.Token))
            .ReturnsAsync(CreateTestPlan(planId));
        _dateTimeProviderMock.SetupGet(p => p.UtcNow).Returns(DateTime.UtcNow);

        // Act
        await _handler.Handle(new CreateSubscriptionOrderCommand(orgId, planId), cts.Token);

        // Assert
        _organizationRepoMock.Verify(r => r.GetByIdAsync(orgId, cts.Token), Times.Once);
        _planRepoMock.Verify(r => r.GetByIdAsync(planId, cts.Token), Times.Once);
    }

    // ===== Helpers =====

    private static Organization CreateTestOrganization(Guid id)
    {
        return Organization.Create(id, Guid.NewGuid(), "TestOrg", "Test about", "Test service description", new List<ServiceType> { ServiceType.InternalMedicine }, DateTime.UtcNow, ReferralSource.Friend, "test@example.com", "support@example.com").Value;
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
