using Viora.Domain.Abstractions;
using Viora.Domain.Orders;
using Viora.Domain.Orders.Internal;
using Viora.Domain.Shared;
using Viora.Domain.Subscriptions.Addons;
using Viora.Domain.Subscriptions.Addons.Internal;

namespace Viora.Test.Compenents.Domain.Orders;

/// <summary>
/// Unit tests for AddonOrder covering the CreateAddonOrder factory method and inherited Order state machine.
/// </summary>
[TestClass]
public sealed class AddonOrderTests
{
    // ===== CreateAddonOrder =====

    /// <summary>
    /// Verifies that CreateAddonOrder with valid addons returns an order with summed price and Draft status.
    /// </summary>
    [TestMethod]
    public void CreateAddonOrder_ValidAddons_ReturnsOrderWithCorrectProperties()
    {
        // Arrange
        Guid orgId = Guid.NewGuid();
        Guid subscriptionId = Guid.NewGuid();
        List<LimitedFeatureAddon> addons = CreateTestAddons(3, 10m, Currency.Usd);
        DateTime createdAt = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        Result<AddonOrder> result = AddonOrder.CreateAddonOrder(orgId, subscriptionId, addons, createdAt);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        AddonOrder order = result.Value;
        Assert.AreEqual(orgId, order.OrganizationId);
        Assert.AreEqual(subscriptionId, order.SubscriptionId);
        Assert.AreEqual(OrderStatus.Draft, order.Status);
        Assert.AreEqual(createdAt, order.CreatedDate);
        Assert.AreEqual(30m, order.TotalPrice.Amount);
        Assert.AreEqual(Currency.Usd, order.TotalPrice.Currency);
    }

    /// <summary>
    /// Verifies that CreateAddonOrder with a single addon sets the price correctly.
    /// </summary>
    [TestMethod]
    public void CreateAddonOrder_SingleAddon_SetsCorrectTotalPrice()
    {
        // Arrange
        List<LimitedFeatureAddon> addons = CreateTestAddons(1, 25m, Currency.Egp);

        // Act
        Result<AddonOrder> result = AddonOrder.CreateAddonOrder(
            Guid.NewGuid(), Guid.NewGuid(), addons, DateTime.UtcNow);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(25m, result.Value.TotalPrice.Amount);
        Assert.AreEqual(Currency.Egp, result.Value.TotalPrice.Currency);
    }

    /// <summary>
    /// Verifies that CreateAddonOrder adds all addons to the Addons collection.
    /// </summary>
    [TestMethod]
    public void CreateAddonOrder_AddsAllToAddonsCollection()
    {
        // Arrange
        List<LimitedFeatureAddon> addons = CreateTestAddons(4, 5m, Currency.Usd);

        // Act
        Result<AddonOrder> result = AddonOrder.CreateAddonOrder(
            Guid.NewGuid(), Guid.NewGuid(), addons, DateTime.UtcNow);

        // Assert
        Assert.AreEqual(4, result.Value.Addons.Count);
    }

    /// <summary>
    /// Verifies that CreateAddonOrder with null addons returns failure with NoAddon error.
    /// </summary>
    [TestMethod]
    public void CreateAddonOrder_NullAddons_ReturnsFailure()
    {
        // Act
        Result<AddonOrder> result = AddonOrder.CreateAddonOrder(
            Guid.NewGuid(), Guid.NewGuid(), null!, DateTime.UtcNow);

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(OrderError.NoAddon, result.Error);
    }

    /// <summary>
    /// Verifies that CreateAddonOrder with an empty addons list returns failure with NoAddon error.
    /// </summary>
    [TestMethod]
    public void CreateAddonOrder_EmptyAddons_ReturnsFailure()
    {
        // Act
        Result<AddonOrder> result = AddonOrder.CreateAddonOrder(
            Guid.NewGuid(), Guid.NewGuid(), [], DateTime.UtcNow);

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(OrderError.NoAddon, result.Error);
    }

    /// <summary>
    /// Verifies that CreateAddonOrder generates a new unique Id.
    /// </summary>
    [TestMethod]
    public void CreateAddonOrder_CalledTwice_GeneratesDifferentIds()
    {
        // Arrange
        List<LimitedFeatureAddon> addons = CreateTestAddons(1, 10m, Currency.Usd);

        // Act
        Result<AddonOrder> result1 = AddonOrder.CreateAddonOrder(
            Guid.NewGuid(), Guid.NewGuid(), addons, DateTime.UtcNow);
        Result<AddonOrder> result2 = AddonOrder.CreateAddonOrder(
            Guid.NewGuid(), Guid.NewGuid(), addons, DateTime.UtcNow);

        // Assert
        Assert.AreNotEqual(result1.Value.Id, result2.Value.Id);
    }

    /// <summary>
    /// Verifies that CreateAddonOrder sets InvoiceId to null.
    /// </summary>
    [TestMethod]
    public void CreateAddonOrder_InvoiceIdIsNull()
    {
        // Arrange
        List<LimitedFeatureAddon> addons = CreateTestAddons(1, 10m, Currency.Usd);

        // Act
        Result<AddonOrder> result = AddonOrder.CreateAddonOrder(
            Guid.NewGuid(), Guid.NewGuid(), addons, DateTime.UtcNow);

        // Assert
        Assert.IsNull(result.Value.InvoiceId);
    }

    // ===== AttachInvoice (inherited from Order) =====

    /// <summary>
    /// Verifies that AttachInvoice on a Draft AddonOrder sets the InvoiceId.
    /// </summary>
    [TestMethod]
    public void AttachInvoice_OnDraftAddonOrder_SetsInvoiceId()
    {
        // Arrange
        AddonOrder order = CreateDraftAddonOrder();
        Guid invoiceId = Guid.NewGuid();

        // Act
        Result result = order.AttachInvoice(invoiceId);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(invoiceId, order.InvoiceId);
    }

    // ===== MarkPending (inherited from Order) =====

    /// <summary>
    /// Verifies that MarkPending on a Draft AddonOrder transitions to Pending.
    /// </summary>
    [TestMethod]
    public void MarkPending_OnDraftAddonOrder_TransitionsToPending()
    {
        // Arrange
        AddonOrder order = CreateDraftAddonOrder();

        // Act
        Result result = order.MarkPending();

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(OrderStatus.Pending, order.Status);
    }

    // ===== MarkPaid (inherited from Order) =====

    /// <summary>
    /// Verifies that MarkPaid on a Pending AddonOrder transitions to Paid.
    /// </summary>
    [TestMethod]
    public void MarkPaid_OnPendingAddonOrder_TransitionsToPaid()
    {
        // Arrange
        AddonOrder order = CreateDraftAddonOrder();
        order.MarkPending();

        // Act
        Result result = order.MarkPaid();

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(OrderStatus.Paid, order.Status);
    }

    /// <summary>
    /// Verifies that MarkPaid on an already Paid AddonOrder returns failure.
    /// </summary>
    [TestMethod]
    public void MarkPaid_OnAlreadyPaidAddonOrder_ReturnsFailure()
    {
        // Arrange
        AddonOrder order = CreateDraftAddonOrder();
        order.MarkPending();
        order.MarkPaid();

        // Act
        Result result = order.MarkPaid();

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(OrderError.AlreadyPaid, result.Error);
    }

    // ===== MarkFailed (inherited from Order) =====

    /// <summary>
    /// Verifies that MarkFailed on a Pending AddonOrder transitions to Failed.
    /// </summary>
    [TestMethod]
    public void MarkFailed_OnPendingAddonOrder_TransitionsToFailed()
    {
        // Arrange
        AddonOrder order = CreateDraftAddonOrder();
        order.MarkPending();

        // Act
        Result result = order.MarkFailed();

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(OrderStatus.Failed, order.Status);
    }

    /// <summary>
    /// Verifies that MarkFailed on a Draft AddonOrder returns failure.
    /// </summary>
    [TestMethod]
    public void MarkFailed_OnDraftAddonOrder_ReturnsFailure()
    {
        // Arrange
        AddonOrder order = CreateDraftAddonOrder();

        // Act
        Result result = order.MarkFailed();

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(OrderError.InvalidStatusTransition, result.Error);
    }

    // ===== Full State Machine Flow =====

    /// <summary>
    /// Verifies the full Draft -> Pending -> Paid flow on an AddonOrder.
    /// </summary>
    [TestMethod]
    public void FullFlow_DraftToPendingToPaid_Succeeds()
    {
        // Arrange
        AddonOrder order = CreateDraftAddonOrder();

        // Act & Assert
        Assert.IsTrue(order.MarkPending().IsSuccess);
        Assert.AreEqual(OrderStatus.Pending, order.Status);

        Assert.IsTrue(order.MarkPaid().IsSuccess);
        Assert.AreEqual(OrderStatus.Paid, order.Status);
    }

    /// <summary>
    /// Verifies the Draft -> Pending -> Failed flow on an AddonOrder.
    /// </summary>
    [TestMethod]
    public void FullFlow_DraftToPendingToFailed_Succeeds()
    {
        // Arrange
        AddonOrder order = CreateDraftAddonOrder();

        // Act & Assert
        Assert.IsTrue(order.MarkPending().IsSuccess);
        Assert.IsTrue(order.MarkFailed().IsSuccess);
        Assert.AreEqual(OrderStatus.Failed, order.Status);
    }

    // ===== Helpers =====

    private static List<LimitedFeatureAddon> CreateTestAddons(int count, decimal pricePerAddon, Currency currency)
    {
        List<LimitedFeatureAddon> addons = [];
        for (int i = 0; i < count; i++)
        {
            addons.Add(LimitedFeatureAddon.Create(
                Guid.NewGuid(), Guid.NewGuid(), AddonType.OneTime, 1, new Money(pricePerAddon, currency)));
        }
        return addons;
    }

    private static AddonOrder CreateDraftAddonOrder()
    {
        List<LimitedFeatureAddon> addons = CreateTestAddons(1, 10m, Currency.Usd);
        return AddonOrder.CreateAddonOrder(
            Guid.NewGuid(), Guid.NewGuid(), addons, DateTime.UtcNow).Value;
    }
}
