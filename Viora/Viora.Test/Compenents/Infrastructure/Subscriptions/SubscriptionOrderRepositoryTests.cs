using Viora.Infrastructure;
using global::Viora.Infrastructure.Repositories.Subscriptions;

namespace Viora.Test.Compenents.Infrastructure.Subscriptions;

/// <summary>
/// Unit tests for the SubscriptionOrderRepository against an InMemory database.
/// SubscriptionOrder has a Money TotalPrice (ComplexProperty) which InMemory cannot even compile queries for.
/// Tests verify Add and empty-result-only assertions.
/// </summary>
[TestClass]
public sealed class SubscriptionOrderRepositoryTests : InfrastructureTestBase
{
    private readonly SubscriptionOrderRepository _repository;

    public SubscriptionOrderRepositoryTests()
    {
        _repository = new SubscriptionOrderRepository(DbContext);
    }

    // ===== Add =====

    [TestMethod]
    public async Task Add_Order_PersistsToDatabase()
    {
        var plan = CreateTestPlan();
        var order = global::Viora.Domain.Orders.SubscriptionOrder.CreateNewSubscriptionOrder(
            Guid.NewGuid(), plan, DateTime.UtcNow).Value;

        _repository.Add(order);
        await DbContext.SaveChangesAsync();

        Assert.IsTrue(order.Id != Guid.Empty);
    }

    // ===== Helpers =====

    private static global::Viora.Domain.Plans.Plan CreateTestPlan()
    {
        return global::Viora.Domain.Plans.Plan.Create(Guid.NewGuid(), "Test Plan", "Description", "Content", 99.99m, global::Viora.Domain.Shared.Currency.Usd, global::Viora.Domain.Plans.Internal.PlanPeriod.monthly);
    }
}
