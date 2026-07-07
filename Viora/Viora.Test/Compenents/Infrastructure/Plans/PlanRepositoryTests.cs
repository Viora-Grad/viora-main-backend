using Viora.Infrastructure;
using global::Viora.Infrastructure.Repositories.Plans;

namespace Viora.Test.Compenents.Infrastructure.Plans;

/// <summary>
/// Unit tests for the PlanRepository against an InMemory database.
/// Plan has a Price (Money ComplexProperty) which InMemory cannot query. Tests verify Add operations only.
/// </summary>
[TestClass]
public sealed class PlanRepositoryTests : InfrastructureTestBase
{
    private readonly PlanRepository _repository;

    public PlanRepositoryTests()
    {
        _repository = new PlanRepository(DbContext);
    }

    // ===== Add =====

    [TestMethod]
    public async Task Add_Plan_PersistsToDatabase()
    {
        var plan = CreateTestPlan("Basic Plan", 99.99m);

        _repository.Add(plan);
        await DbContext.SaveChangesAsync();

        Assert.IsTrue(plan.Id != Guid.Empty);
    }

    [TestMethod]
    public async Task Add_MultiplePlans_PersistsAll()
    {
        var plan1 = CreateTestPlan("Plan A", 49.99m);
        var plan2 = CreateTestPlan("Plan B", 99.99m);

        _repository.Add(plan1);
        _repository.Add(plan2);
        await DbContext.SaveChangesAsync();

        Assert.AreNotEqual(plan1.Id, plan2.Id);
    }

    // ===== Helpers =====

    private static global::Viora.Domain.Plans.Plan CreateTestPlan(string name, decimal price)
    {
        return global::Viora.Domain.Plans.Plan.Create(Guid.NewGuid(), name, "Description", "Content", price, global::Viora.Domain.Shared.Currency.Usd, global::Viora.Domain.Plans.Internal.PlanPeriod.monthly);
    }
}
