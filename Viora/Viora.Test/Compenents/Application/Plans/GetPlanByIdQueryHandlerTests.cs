using System.Reflection;
using Moq;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Plans.GetPlanById;
using Viora.Application.Plans.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Features;
using Viora.Domain.Plans.Internal;
using Viora.Domain.Shared;

namespace Viora.Test.Compenents.Application.Plans;

/// <summary>
/// Unit tests for the GetPlanByIdQueryHandler covering successful retrieval and not-found scenarios.
/// </summary>
[TestClass]
public sealed class GetPlanByIdQueryHandlerTests
{
    private readonly Mock<IPlanRepository> _planRepoMock = new();
    private readonly GetPlanByIdQueryHandler _handler;

    public GetPlanByIdQueryHandlerTests()
    {
        _handler = new GetPlanByIdQueryHandler(_planRepoMock.Object);
    }

    // ===== Handle =====

    /// <summary>
    /// Verifies that Handle with a valid plan ID returns a PlanResponse with correct properties.
    /// </summary>
    [TestMethod]
    public async Task Handle_ValidPlanId_ReturnsPlanResponseWithCorrectProperties()
    {
        // Arrange
        Guid planId = Guid.NewGuid();
        Plan plan = CreateTestPlan(planId);
        var query = new GetPlanByIdQuery(planId);

        _planRepoMock.Setup(r => r.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        // Act
        Result<PlanResponse> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Value);
        Assert.AreEqual(planId, result.Value.Id);
        Assert.AreEqual("Test Plan", result.Value.Name);
        Assert.AreEqual("Test Description", result.Value.Description);
        Assert.AreEqual("Test Content", result.Value.PlanContent);
        Assert.AreEqual(99.99m, result.Value.Price.amount);
        Assert.AreEqual("USD", result.Value.Price.currency);
        Assert.AreEqual("Monthly", result.Value.PlanPeriodTime);
    }

    /// <summary>
    /// Verifies that Handle with a valid plan ID returns features mapped correctly.
    /// </summary>
    [TestMethod]
    public async Task Handle_PlanWithFeatures_ReturnsMappedFeatures()
    {
        // Arrange
        Guid planId = Guid.NewGuid();
        Plan plan = CreateTestPlan(planId);

        Guid featureId = Guid.NewGuid();
        Feature feature = Feature.Create(featureId, "appointments", "Appointment scheduling");
        PlanFeature planFeature = PlanFeature.Create(Guid.NewGuid(), planId, featureId);
        SetPlanFeatureNavigation(planFeature, feature);
        SetPlanFeatures(plan, new List<PlanFeature> { planFeature });

        var query = new GetPlanByIdQuery(planId);

        _planRepoMock.Setup(r => r.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        // Act
        Result<PlanResponse> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.Value.Features.Count);
        Assert.AreEqual(featureId, result.Value.Features[0].Id);
        Assert.AreEqual("appointments", result.Value.Features[0].Key);
        Assert.AreEqual("Appointment scheduling", result.Value.Features[0].Description);
    }

    /// <summary>
    /// Verifies that Handle with a valid plan ID returns limited features mapped correctly.
    /// </summary>
    [TestMethod]
    public async Task Handle_PlanWithLimitedFeatures_ReturnsMappedLimitedFeatures()
    {
        // Arrange
        Guid planId = Guid.NewGuid();
        Plan plan = CreateTestPlan(planId);

        LimitedFeature limitedFeature = LimitedFeature.StaffMembers;
        PlanLimitedFeature planLimitedFeature = PlanLimitedFeature.Create(Guid.NewGuid(), planId, limitedFeature.Id, 10);
        SetPlanLimitedFeatureNavigation(planLimitedFeature, limitedFeature);
        SetPlanLimitedFeatures(plan, new List<PlanLimitedFeature> { planLimitedFeature });

        var query = new GetPlanByIdQuery(planId);

        _planRepoMock.Setup(r => r.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        // Act
        Result<PlanResponse> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.Value.LimitedFeatures.Count);
        Assert.AreEqual(limitedFeature.Id, result.Value.LimitedFeatures[0].Id);
        Assert.AreEqual("staff_members", result.Value.LimitedFeatures[0].Key);
        Assert.AreEqual(10, result.Value.LimitedFeatures[0].Limit);
    }

    /// <summary>
    /// Verifies that Handle with a valid plan ID returns empty features and limited features when none exist.
    /// </summary>
    [TestMethod]
    public async Task Handle_PlanWithNoFeatures_ReturnsEmptyCollections()
    {
        // Arrange
        Guid planId = Guid.NewGuid();
        Plan plan = CreateTestPlan(planId);
        var query = new GetPlanByIdQuery(planId);

        _planRepoMock.Setup(r => r.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        // Act
        Result<PlanResponse> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0, result.Value.Features.Count);
        Assert.AreEqual(0, result.Value.LimitedFeatures.Count);
    }

    /// <summary>
    /// Verifies that Handle with a non-existent plan ID throws NotFoundException.
    /// </summary>
    [TestMethod]
    public async Task Handle_PlanNotFound_ThrowsNotFoundException()
    {
        // Arrange
        Guid planId = Guid.NewGuid();
        var query = new GetPlanByIdQuery(planId);

        _planRepoMock.Setup(r => r.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Plan?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(query, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that Handle passes the cancellation token to the repository.
    /// </summary>
    [TestMethod]
    public async Task Handle_CallsRepositoryWithCancellationToken()
    {
        // Arrange
        Guid planId = Guid.NewGuid();
        Plan plan = CreateTestPlan(planId);
        var query = new GetPlanByIdQuery(planId);
        var cts = new CancellationTokenSource();

        _planRepoMock.Setup(r => r.GetByIdAsync(planId, cts.Token))
            .ReturnsAsync(plan);

        // Act
        await _handler.Handle(query, cts.Token);

        // Assert
        _planRepoMock.Verify(r => r.GetByIdAsync(planId, cts.Token), Times.Once);
    }

    /// <summary>
    /// Verifies that Handle returns a PlanResponse with multiple features and limited features.
    /// </summary>
    [TestMethod]
    public async Task Handle_PlanWithMultipleFeaturesAndLimitedFeatures_ReturnsAllMapped()
    {
        // Arrange
        Guid planId = Guid.NewGuid();
        Plan plan = CreateTestPlan(planId);

        Feature f1 = Feature.Create(Guid.NewGuid(), "feature_a", "Feature A");
        Feature f2 = Feature.Create(Guid.NewGuid(), "feature_b", "Feature B");
        PlanFeature pf1 = PlanFeature.Create(Guid.NewGuid(), planId, f1.Id);
        PlanFeature pf2 = PlanFeature.Create(Guid.NewGuid(), planId, f2.Id);
        SetPlanFeatureNavigation(pf1, f1);
        SetPlanFeatureNavigation(pf2, f2);
        SetPlanFeatures(plan, new List<PlanFeature> { pf1, pf2 });

        LimitedFeature lf1 = LimitedFeature.Branches;
        LimitedFeature lf2 = LimitedFeature.StorageBytes;
        PlanLimitedFeature plf1 = PlanLimitedFeature.Create(Guid.NewGuid(), planId, lf1.Id, 5);
        PlanLimitedFeature plf2 = PlanLimitedFeature.Create(Guid.NewGuid(), planId, lf2.Id, 1073741824);
        SetPlanLimitedFeatureNavigation(plf1, lf1);
        SetPlanLimitedFeatureNavigation(plf2, lf2);
        SetPlanLimitedFeatures(plan, new List<PlanLimitedFeature> { plf1, plf2 });

        var query = new GetPlanByIdQuery(planId);

        _planRepoMock.Setup(r => r.GetByIdAsync(planId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        // Act
        Result<PlanResponse> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(2, result.Value.Features.Count);
        Assert.AreEqual(2, result.Value.LimitedFeatures.Count);
    }

    // ===== Helpers =====

    private static Plan CreateTestPlan(Guid? id = null)
    {
        return Plan.Create(
            id ?? Guid.NewGuid(),
            "Test Plan",
            "Test Description",
            "Test Content",
            99.99m,
            Currency.Usd,
            PlanPeriod.monthly);
    }

    private static void SetPlanFeatures(Plan plan, List<PlanFeature> features)
    {
        typeof(Plan).GetProperty("PlanFeatures", BindingFlags.Public | BindingFlags.Instance)!
            .SetValue(plan, features);
    }

    private static void SetPlanLimitedFeatures(Plan plan, List<PlanLimitedFeature> limitedFeatures)
    {
        typeof(Plan).GetProperty("PlanLimitedFeatures", BindingFlags.Public | BindingFlags.Instance)!
            .SetValue(plan, limitedFeatures);
    }

    private static void SetPlanFeatureNavigation(PlanFeature planFeature, Feature feature)
    {
        typeof(PlanFeature).GetProperty("Feature", BindingFlags.Public | BindingFlags.Instance)!
            .SetValue(planFeature, feature);
    }

    private static void SetPlanLimitedFeatureNavigation(PlanLimitedFeature planLimitedFeature, LimitedFeature limitedFeature)
    {
        typeof(PlanLimitedFeature).GetProperty("LimitedFeature", BindingFlags.Public | BindingFlags.Instance)!
            .SetValue(planLimitedFeature, limitedFeature);
    }
}
