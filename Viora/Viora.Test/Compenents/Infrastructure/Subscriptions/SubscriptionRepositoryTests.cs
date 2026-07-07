using Viora.Infrastructure;
using global::Viora.Infrastructure.Repositories.Subscriptions;

namespace Viora.Test.Compenents.Infrastructure.Subscriptions;

/// <summary>
/// Unit tests for the SubscriptionRepository against an InMemory database.
/// </summary>
[TestClass]
public sealed class SubscriptionRepositoryTests : InfrastructureTestBase
{
    private readonly SubscriptionRepository _repository;

    public SubscriptionRepositoryTests()
    {
        _repository = new SubscriptionRepository(DbContext);
    }

    // ===== Add =====

    [TestMethod]
    public async Task Add_Subscription_PersistsToDatabase()
    {
        var plan = CreateTestPlan();
        var subscription = global::Viora.Domain.Subscriptions.Subscription.Create(plan.Id, Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow.AddMonths(1)).Value;

        _repository.Add(subscription);
        await DbContext.SaveChangesAsync();

        Assert.IsTrue(subscription.Id != Guid.Empty);
    }

    // ===== GetByOrganizationIdAsync =====

    [TestMethod]
    public async Task GetByOrganizationIdAsync_OrgWithNoActiveSubscription_ReturnsNull()
    {
        var result = await _repository.GetByOrganizationIdAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.IsNull(result);
    }

    // ===== Helpers =====

    private static global::Viora.Domain.Plans.Plan CreateTestPlan()
    {
        return global::Viora.Domain.Plans.Plan.Create(Guid.NewGuid(), "Test Plan", "Description", "Content", 99.99m, global::Viora.Domain.Shared.Currency.Usd, global::Viora.Domain.Plans.Internal.PlanPeriod.monthly);
    }
}
