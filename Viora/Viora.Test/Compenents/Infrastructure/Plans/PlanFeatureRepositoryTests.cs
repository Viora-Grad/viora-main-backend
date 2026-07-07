using Viora.Infrastructure;
using global::Viora.Infrastructure.Repositories.Plans;

namespace Viora.Test.Compenents.Infrastructure.Plans;

/// <summary>
/// Unit tests for the PlanFeatureRepository against an InMemory database.
/// PlanFeature has no ComplexProperty so queries work normally.
/// </summary>
[TestClass]
public sealed class PlanFeatureRepositoryTests : InfrastructureTestBase
{
    private readonly PlanFeatureRepository _repository;

    public PlanFeatureRepositoryTests()
    {
        _repository = new PlanFeatureRepository(DbContext);
    }

    // ===== GetByPlanIdAsync =====

    [TestMethod]
    public async Task GetByPlanIdAsync_PlanWithNoFeatures_ReturnsEmpty()
    {
        var result = await _repository.GetByPlanIdAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetByPlanIdAsync_NonExistentPlan_ReturnsEmpty()
    {
        var result = await _repository.GetByPlanIdAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.AreEqual(0, result.Count);
    }

    // ===== GetByPlanIdsAsync =====

    [TestMethod]
    public async Task GetByPlanIdsAsync_EmptyList_ReturnsEmpty()
    {
        var result = await _repository.GetByPlanIdsAsync(new List<Guid>(), CancellationToken.None);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetByPlanIdsAsync_NonExistentIds_ReturnsEmpty()
    {
        var result = await _repository.GetByPlanIdsAsync(new List<Guid> { Guid.NewGuid() }, CancellationToken.None);
        Assert.AreEqual(0, result.Count);
    }
}
