using Viora.Domain.Abstractions;
using Viora.Domain.Orders;
using Viora.Domain.Orders.Internal;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Internal;
using Viora.Domain.Shared;

namespace Viora.Test.Compenents.Domain.Orders;

/// <summary>
/// Unit tests for SubscriptionOrder covering factory methods and inherited Order state machine.
/// </summary>
[TestClass]
public sealed class SubscriptionOrderTests
{
    // ===== CreateNewSubscriptionOrder =====

    /// <summary>
    /// Verifies that CreateNewSubscriptionOrder returns a SubscriptionOrder with NewSubscription type and Draft status.
    /// </summary>
    [TestMethod]
    public void CreateNewSubscriptionOrder_ValidInput_ReturnsOrderWithCorrectProperties()
    {
        // Arrange
        Guid orgId = Guid.NewGuid();
        Plan plan = CreateTestPlan();
        DateTime createdAt = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        Result<SubscriptionOrder> result = SubscriptionOrder.CreateNewSubscriptionOrder(orgId, plan, createdAt);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        SubscriptionOrder order = result.Value;
        Assert.AreEqual(orgId, order.OrganizationId);
        Assert.AreEqual(plan.Id, order.PlanId);
        Assert.AreEqual(SubscriptionOrderType.NewSubscription, order.SubscriptionOrderType);
        Assert.AreEqual(OrderStatus.Draft, order.Status);
        Assert.AreEqual(createdAt, order.CreatedDate);
        Assert.IsNull(order.SubscriptionId);
        Assert.AreEqual(plan.Price.Amount, order.TotalPrice.Amount);
        Assert.AreEqual(plan.Price.Currency, order.TotalPrice.Currency);
    }

    /// <summary>
    /// Verifies that CreateNewSubscriptionOrder generates a new unique Id.
    /// </summary>
    [TestMethod]
    public void CreateNewSubscriptionOrder_CalledTwice_GeneratesDifferentIds()
    {
        // Arrange
        Plan plan = CreateTestPlan();
        DateTime createdAt = DateTime.UtcNow;

        // Act
        Result<SubscriptionOrder> result1 = SubscriptionOrder.CreateNewSubscriptionOrder(Guid.NewGuid(), plan, createdAt);
        Result<SubscriptionOrder> result2 = SubscriptionOrder.CreateNewSubscriptionOrder(Guid.NewGuid(), plan, createdAt);

        // Assert
        Assert.AreNotEqual(result1.Value.Id, result2.Value.Id);
    }

    /// <summary>
    /// Verifies that CreateNewSubscriptionOrder sets InvoiceId to null.
    /// </summary>
    [TestMethod]
    public void CreateNewSubscriptionOrder_InvoiceIdIsNull()
    {
        // Arrange & Act
        Result<SubscriptionOrder> result = SubscriptionOrder.CreateNewSubscriptionOrder(
            Guid.NewGuid(), CreateTestPlan(), DateTime.UtcNow);

        // Assert
        Assert.IsNull(result.Value.InvoiceId);
    }

    // ===== CreateRenewSubscriptionOrder =====

    /// <summary>
    /// Verifies that CreateRenewSubscriptionOrder returns an order with Renewal type and the subscriptionId.
    /// </summary>
    [TestMethod]
    public void CreateRenewSubscriptionOrder_ValidInput_ReturnsOrderWithCorrectProperties()
    {
        // Arrange
        Guid orgId = Guid.NewGuid();
        Guid planId = Guid.NewGuid();
        Guid subscriptionId = Guid.NewGuid();
        Money totalPrice = new(50m, Currency.Usd);
        DateTime createdAt = new(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        Result<SubscriptionOrder> result = SubscriptionOrder.CreateRenewSubscriptionOrder(
            orgId, planId, subscriptionId, totalPrice, createdAt);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        SubscriptionOrder order = result.Value;
        Assert.AreEqual(orgId, order.OrganizationId);
        Assert.AreEqual(planId, order.PlanId);
        Assert.AreEqual(subscriptionId, order.SubscriptionId);
        Assert.AreEqual(SubscriptionOrderType.Renewal, order.SubscriptionOrderType);
        Assert.AreEqual(OrderStatus.Draft, order.Status);
        Assert.AreEqual(totalPrice.Amount, order.TotalPrice.Amount);
    }

    /// <summary>
    /// Verifies that CreateRenewSubscriptionOrder generates a new unique Id.
    /// </summary>
    [TestMethod]
    public void CreateRenewSubscriptionOrder_CalledTwice_GeneratesDifferentIds()
    {
        // Arrange
        Guid planId = Guid.NewGuid();
        Guid subscriptionId = Guid.NewGuid();
        Money price = new(25m, Currency.Egp);

        // Act
        Result<SubscriptionOrder> result1 = SubscriptionOrder.CreateRenewSubscriptionOrder(
            Guid.NewGuid(), planId, subscriptionId, price, DateTime.UtcNow);
        Result<SubscriptionOrder> result2 = SubscriptionOrder.CreateRenewSubscriptionOrder(
            Guid.NewGuid(), planId, subscriptionId, price, DateTime.UtcNow);

        // Assert
        Assert.AreNotEqual(result1.Value.Id, result2.Value.Id);
    }

    // ===== CreateChangeSubscriptionOrder =====

    /// <summary>
    /// Verifies that CreateChangeSubscriptionOrder returns an order with ChangeSubscription type and the new plan.
    /// </summary>
    [TestMethod]
    public void CreateChangeSubscriptionOrder_ValidInput_ReturnsOrderWithCorrectProperties()
    {
        // Arrange
        Guid orgId = Guid.NewGuid();
        Guid subscriptionId = Guid.NewGuid();
        Plan newPlan = CreateTestPlan();
        DateTime createdAt = new(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc);

        // Act
        Result<SubscriptionOrder> result = SubscriptionOrder.CreateChangeSubscriptionOrder(
            orgId, subscriptionId, newPlan, createdAt);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        SubscriptionOrder order = result.Value;
        Assert.AreEqual(orgId, order.OrganizationId);
        Assert.AreEqual(subscriptionId, order.SubscriptionId);
        Assert.AreEqual(newPlan.Id, order.PlanId);
        Assert.AreEqual(SubscriptionOrderType.ChangeSubscription, order.SubscriptionOrderType);
        Assert.AreEqual(OrderStatus.Draft, order.Status);
        Assert.AreEqual(newPlan.Price.Amount, order.TotalPrice.Amount);
    }

    /// <summary>
    /// Verifies that CreateChangeSubscriptionOrder generates a new unique Id.
    /// </summary>
    [TestMethod]
    public void CreateChangeSubscriptionOrder_CalledTwice_GeneratesDifferentIds()
    {
        // Arrange
        Plan plan = CreateTestPlan();

        // Act
        Result<SubscriptionOrder> result1 = SubscriptionOrder.CreateChangeSubscriptionOrder(
            Guid.NewGuid(), Guid.NewGuid(), plan, DateTime.UtcNow);
        Result<SubscriptionOrder> result2 = SubscriptionOrder.CreateChangeSubscriptionOrder(
            Guid.NewGuid(), Guid.NewGuid(), plan, DateTime.UtcNow);

        // Assert
        Assert.AreNotEqual(result1.Value.Id, result2.Value.Id);
    }

    // ===== AttachInvoice (inherited from Order) =====

    /// <summary>
    /// Verifies that AttachInvoice on a Draft order sets the InvoiceId and returns success.
    /// </summary>
    [TestMethod]
    public void AttachInvoice_OnDraftOrder_SetsInvoiceIdAndReturnsSuccess()
    {
        // Arrange
        SubscriptionOrder order = CreateDraftOrder();
        Guid invoiceId = Guid.NewGuid();

        // Act
        Result result = order.AttachInvoice(invoiceId);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(invoiceId, order.InvoiceId);
    }

    /// <summary>
    /// Verifies that AttachInvoice on a non-Draft order returns failure with InvalidStatusTransition.
    /// </summary>
    [TestMethod]
    public void AttachInvoice_OnNonDraftOrder_ReturnsFailure()
    {
        // Arrange
        SubscriptionOrder order = CreateDraftOrder();
        order.MarkPending();
        Guid invoiceId = Guid.NewGuid();

        // Act
        Result result = order.AttachInvoice(invoiceId);

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(OrderError.InvalidStatusTransition, result.Error);
    }

    /// <summary>
    /// Verifies that AttachInvoice when an invoice is already attached returns failure.
    /// </summary>
    [TestMethod]
    public void AttachInvoice_WhenInvoiceAlreadyAttached_ReturnsFailure()
    {
        // Arrange
        SubscriptionOrder order = CreateDraftOrder();
        order.AttachInvoice(Guid.NewGuid());

        // Act
        Result result = order.AttachInvoice(Guid.NewGuid());

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(OrderError.InvoiceAlreadyAttached, result.Error);
    }

    // ===== MarkPending (inherited from Order) =====

    /// <summary>
    /// Verifies that MarkPending on a Draft order transitions status to Pending.
    /// </summary>
    [TestMethod]
    public void MarkPending_OnDraftOrder_TransitionsToPending()
    {
        // Arrange
        SubscriptionOrder order = CreateDraftOrder();

        // Act
        Result result = order.MarkPending();

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(OrderStatus.Pending, order.Status);
    }

    /// <summary>
    /// Verifies that MarkPending on a non-Draft order returns failure.
    /// </summary>
    [TestMethod]
    public void MarkPending_OnNonDraftOrder_ReturnsFailure()
    {
        // Arrange
        SubscriptionOrder order = CreateDraftOrder();
        order.MarkPending();

        // Act
        Result result = order.MarkPending();

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(OrderError.InvalidStatusTransition, result.Error);
    }

    // ===== MarkPaid (inherited from Order) =====

    /// <summary>
    /// Verifies that MarkPaid on a Pending order transitions status to Paid.
    /// </summary>
    [TestMethod]
    public void MarkPaid_OnPendingOrder_TransitionsToPaid()
    {
        // Arrange
        SubscriptionOrder order = CreateDraftOrder();
        order.MarkPending();

        // Act
        Result result = order.MarkPaid();

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(OrderStatus.Paid, order.Status);
    }

    /// <summary>
    /// Verifies that MarkPaid on a Draft order returns failure with InvalidStatusTransition.
    /// </summary>
    [TestMethod]
    public void MarkPaid_OnDraftOrder_ReturnsFailure()
    {
        // Arrange
        SubscriptionOrder order = CreateDraftOrder();

        // Act
        Result result = order.MarkPaid();

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(OrderError.InvalidStatusTransition, result.Error);
    }

    /// <summary>
    /// Verifies that MarkPaid on an already Paid order returns failure with AlreadyPaid.
    /// </summary>
    [TestMethod]
    public void MarkPaid_OnAlreadyPaidOrder_ReturnsFailure()
    {
        // Arrange
        SubscriptionOrder order = CreateDraftOrder();
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
    /// Verifies that MarkFailed on a Pending order transitions status to Failed.
    /// </summary>
    [TestMethod]
    public void MarkFailed_OnPendingOrder_TransitionsToFailed()
    {
        // Arrange
        SubscriptionOrder order = CreateDraftOrder();
        order.MarkPending();

        // Act
        Result result = order.MarkFailed();

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(OrderStatus.Failed, order.Status);
    }

    /// <summary>
    /// Verifies that MarkFailed on a Draft order returns failure.
    /// </summary>
    [TestMethod]
    public void MarkFailed_OnDraftOrder_ReturnsFailure()
    {
        // Arrange
        SubscriptionOrder order = CreateDraftOrder();

        // Act
        Result result = order.MarkFailed();

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(OrderError.InvalidStatusTransition, result.Error);
    }

    /// <summary>
    /// Verifies that MarkFailed on a Paid order returns failure.
    /// </summary>
    [TestMethod]
    public void MarkFailed_OnPaidOrder_ReturnsFailure()
    {
        // Arrange
        SubscriptionOrder order = CreateDraftOrder();
        order.MarkPending();
        order.MarkPaid();

        // Act
        Result result = order.MarkFailed();

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(OrderError.InvalidStatusTransition, result.Error);
    }

    // ===== Helpers =====

    private static Plan CreateTestPlan()
    {
        return Plan.Create(
            Guid.NewGuid(), "Test Plan", "A test plan", "Content",
            29.99m, Currency.Usd, PlanPeriod.monthly);
    }

    private static SubscriptionOrder CreateDraftOrder()
    {
        return SubscriptionOrder.CreateNewSubscriptionOrder(
            Guid.NewGuid(), CreateTestPlan(), DateTime.UtcNow).Value;
    }
}
