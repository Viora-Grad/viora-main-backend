using Moq;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Orders.GetOrganizationSubscriptionOrders;
using Viora.Domain.Abstractions;
using Viora.Domain.Orders;
using Viora.Domain.Orders.Internal;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Organizations.Shared.Enums;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Internal;
using Viora.Domain.Shared;

namespace Viora.Test.Compenents.Application.Orders;

/// <summary>
/// Unit tests for the GetOrganizationSubscriptionOrdersQueryHandler covering successful retrieval and not-found scenarios.
/// </summary>
[TestClass]
public sealed class GetOrganizationSubscriptionOrdersQueryHandlerTests
{
    private readonly Mock<ISubscriptionOrderRepository> _subscriptionOrderRepoMock = new();
    private readonly Mock<IOrganizationRepository> _organizationRepoMock = new();
    private readonly Mock<IPlanRepository> _planRepoMock = new();
    private readonly GetOrganizationSubscriptionOrdersQueryHandler _handler;

    public GetOrganizationSubscriptionOrdersQueryHandlerTests()
    {
        _handler = new GetOrganizationSubscriptionOrdersQueryHandler(
            _subscriptionOrderRepoMock.Object,
            _organizationRepoMock.Object,
            _planRepoMock.Object);
    }

    // ===== Handle =====

    /// <summary>
    /// Verifies that Handle with non-existent organization throws NotFoundException.
    /// </summary>
    [TestMethod]
    public async Task Handle_OrganizationNotFound_ThrowsNotFoundException()
    {
        // Arrange
        Guid orgId = Guid.NewGuid();

        _organizationRepoMock.Setup(r => r.GetByIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organization?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new GetOrganizationSubscriptionOrdersQuery(orgId), CancellationToken.None));
    }

    /// <summary>
    /// Verifies that Handle with no orders returns an empty list.
    /// </summary>
    [TestMethod]
    public async Task Handle_NoOrders_ReturnsEmptyList()
    {
        // Arrange
        Guid orgId = Guid.NewGuid();
        var org = CreateTestOrganization(orgId);

        _organizationRepoMock.Setup(r => r.GetByIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _subscriptionOrderRepoMock.Setup(r => r.GetAllByOrganizationIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SubscriptionOrder>());
        _planRepoMock.Setup(r => r.GetAllAsNoTrackingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Plan>());

        // Act
        var result = await _handler.Handle(
            new GetOrganizationSubscriptionOrdersQuery(orgId), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0, result.Value.Count);
    }

    /// <summary>
    /// Verifies that Handle with orders returns mapped responses with resolved plan names.
    /// </summary>
    [TestMethod]
    public async Task Handle_WithOrders_ReturnsMappedResponse()
    {
        // Arrange
        Guid orgId = Guid.NewGuid();
        Guid planId = Guid.NewGuid();
        var org = CreateTestOrganization(orgId);

        var order = CreateTestSubscriptionOrder(orgId, planId);
        var plan = CreateTestPlan(planId, "Basic Plan");

        _organizationRepoMock.Setup(r => r.GetByIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _subscriptionOrderRepoMock.Setup(r => r.GetAllByOrganizationIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SubscriptionOrder> { order });
        _planRepoMock.Setup(r => r.GetAllAsNoTrackingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Plan> { plan });

        // Act
        var result = await _handler.Handle(
            new GetOrganizationSubscriptionOrdersQuery(orgId), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.Value.Count);

        var response = result.Value[0];
        Assert.AreEqual(order.Id, response.Id);
        Assert.AreEqual(orgId, response.OrganizationId);
        Assert.AreEqual(order.SubscriptionId, response.SubscriptionId);
        Assert.AreEqual(order.InvoiceId, response.InvoiceId);
        Assert.AreEqual(planId, response.PlanId);
        Assert.AreEqual("Basic Plan", response.PlanName);
        Assert.AreEqual(order.TotalPrice, response.Price);
        Assert.AreEqual(order.CreatedDate, response.CreatedAtUtc);
        Assert.AreEqual(order.SubscriptionOrderType.Value, response.OrderType);
        Assert.AreEqual(order.Status.Value, response.Status);
    }

    // ===== Helpers =====

    private static Organization CreateTestOrganization(Guid id)
    {
        return Organization.Create(id, Guid.NewGuid(), "TestOrg", "Test about", "Test service description", new List<ServiceType> { ServiceType.InternalMedicine }, DateTime.UtcNow, ReferralSource.Friend, "test@example.com", "support@example.com").Value;
    }

    private static SubscriptionOrder CreateTestSubscriptionOrder(Guid orgId, Guid planId)
    {
        var result = SubscriptionOrder.CreateRenewSubscriptionOrder(
            orgId,
            planId,
            Guid.NewGuid(),
            new Money(99.99m, Currency.Usd),
            DateTime.UtcNow);

        return result.Value;
    }

    private static Plan CreateTestPlan(Guid id, string name)
    {
        return Plan.Create(
            id,
            name,
            "Description",
            "Content",
            99.99m,
            Currency.Usd,
            PlanPeriod.monthly);
    }
}
