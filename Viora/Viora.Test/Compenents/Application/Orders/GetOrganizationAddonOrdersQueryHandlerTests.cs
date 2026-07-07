using Moq;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Orders.GetOrganizationAddonOrders;
using Viora.Domain.Abstractions;
using Viora.Domain.Orders;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Organizations.Shared.Enums;
using Viora.Domain.Shared;
using Viora.Domain.Subscriptions.Addons;
using Viora.Domain.Subscriptions.Addons.Internal;

namespace Viora.Test.Compenents.Application.Orders;

/// <summary>
/// Unit tests for the GetOrganizationAddonOrdersQueryHandler covering successful retrieval and not-found scenarios.
/// </summary>
[TestClass]
public sealed class GetOrganizationAddonOrdersQueryHandlerTests
{
    private readonly Mock<IAddonOrderRepository> _addonOrderRepoMock = new();
    private readonly Mock<IOrganizationRepository> _organizationRepoMock = new();
    private readonly GetOrganizationAddonOrdersQueryHandler _handler;

    public GetOrganizationAddonOrdersQueryHandlerTests()
    {
        _handler = new GetOrganizationAddonOrdersQueryHandler(
            _addonOrderRepoMock.Object,
            _organizationRepoMock.Object);
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
            _handler.Handle(new GetOrganizationAddonOrdersQuery(orgId), CancellationToken.None));
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
        _addonOrderRepoMock.Setup(r => r.GetAllByOrganizationIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AddonOrder>());

        // Act
        var result = await _handler.Handle(
            new GetOrganizationAddonOrdersQuery(orgId), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0, result.Value.Count);
    }

    /// <summary>
    /// Verifies that Handle with orders returns mapped responses with addon details.
    /// </summary>
    [TestMethod]
    public async Task Handle_WithOrders_ReturnsMappedResponse()
    {
        // Arrange
        Guid orgId = Guid.NewGuid();
        var org = CreateTestOrganization(orgId);

        var addon = LimitedFeatureAddon.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            AddonType.TimeBase,
            5,
            new Money(19.99m, Currency.Usd));

        var orderResult = AddonOrder.CreateAddonOrder(
            orgId,
            Guid.NewGuid(),
            new List<LimitedFeatureAddon> { addon },
            DateTime.UtcNow);
        var order = orderResult.Value;

        _organizationRepoMock.Setup(r => r.GetByIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(org);
        _addonOrderRepoMock.Setup(r => r.GetAllByOrganizationIdAsync(orgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AddonOrder> { order });

        // Act
        var result = await _handler.Handle(
            new GetOrganizationAddonOrdersQuery(orgId), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.Value.Count);

        var response = result.Value[0];
        Assert.AreEqual(order.Id, response.Id);
        Assert.AreEqual(orgId, response.OrganizationId);
        Assert.AreEqual(order.SubscriptionId, response.SubscriptionId);
        Assert.AreEqual(order.InvoiceId, response.InvoiceId);
        Assert.AreEqual(order.Status.Value, response.Status);
        Assert.AreEqual(order.TotalPrice, response.Price);
        Assert.AreEqual(order.CreatedDate, response.CreatedDate);
        Assert.AreEqual(1, response.Addons.Count);
        Assert.AreEqual(addon.Id, response.Addons[0].Id);
        Assert.AreEqual(addon.LimitedFeatureId, response.Addons[0].LimitedFeatureId);
        Assert.AreEqual(addon.AddonType.Value, response.Addons[0].AddonType);
        Assert.AreEqual(addon.RestoreValue, response.Addons[0].RestoreValue);
        Assert.AreEqual(addon.Price, response.Addons[0].Price);
    }

    // ===== Helpers =====

    private static Organization CreateTestOrganization(Guid id)
    {
        return Organization.Create(id, Guid.NewGuid(), "TestOrg", "Test about", "Test service description", new List<ServiceType> { ServiceType.InternalMedicine }, DateTime.UtcNow, ReferralSource.Friend, "test@example.com", "support@example.com").Value;
    }
}
