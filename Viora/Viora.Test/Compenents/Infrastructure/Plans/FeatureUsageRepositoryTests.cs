using Viora.Infrastructure;
using global::Viora.Infrastructure.Repositories.Plans;

namespace Viora.Test.Compenents.Infrastructure.Plans;

/// <summary>
/// Unit tests for the FeatureUsageRepository against an InMemory database.
/// FeatureUsage has no ComplexProperty so queries work normally, but requires RowVersion for persistence.
/// </summary>
[TestClass]
public sealed class FeatureUsageRepositoryTests : InfrastructureTestBase
{
    private readonly FeatureUsageRepository _repository;

    public FeatureUsageRepositoryTests()
    {
        _repository = new FeatureUsageRepository(DbContext);
    }

    // ===== GetByOrganizationIdAndFeatureIdAsync =====

    [TestMethod]
    public async Task GetByOrganizationIdAndFeatureIdAsync_NotExists_ReturnsNull()
    {
        var result = await _repository.GetByOrganizationIdAndFeatureIdAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        Assert.IsNull(result);
    }

    // ===== GetByOrganizationIdAsync =====

    [TestMethod]
    public async Task GetByOrganizationIdAsync_Empty_ReturnsEmpty()
    {
        var result = await _repository.GetByOrganizationIdAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.AreEqual(0, result.Count);
    }
}
