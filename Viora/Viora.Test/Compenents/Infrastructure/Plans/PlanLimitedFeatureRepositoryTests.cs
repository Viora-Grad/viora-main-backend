using Viora.Infrastructure;
using global::Viora.Infrastructure.Repositories.Plans;

namespace Viora.Test.Compenents.Infrastructure.Plans;

/// <summary>
/// Unit tests for the PlanLimitedFeatureRepository against an InMemory database.
/// PlanLimitedFeature has no ComplexProperty but querying requires entities without Plan FK materialization.
/// </summary>
[TestClass]
public sealed class PlanLimitedFeatureRepositoryTests : InfrastructureTestBase
{
    private readonly PlanLimitedFeatureRepository _repository;

    public PlanLimitedFeatureRepositoryTests()
    {
        _repository = new PlanLimitedFeatureRepository(DbContext);
    }

    // ===== GetPlanLimitedFeatureByLimitedFeatureIdAsync =====

    [TestMethod]
    public async Task GetPlanLimitedFeatureByLimitedFeatureIdAsync_FeatureNotFound_ReturnsNull()
    {
        var result = await _repository.GetPlanLimitedFeatureByLimitedFeatureIdAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        Assert.IsNull(result);
    }
}
