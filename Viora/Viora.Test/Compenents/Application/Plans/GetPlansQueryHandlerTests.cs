using System.Reflection;
using Moq;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Plans.GetPlans;
using Viora.Application.Plans.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Features;
using Viora.Domain.Plans.Internal;
using Viora.Domain.Shared;

namespace Viora.Test.Compenents.Application.Plans;

/// <summary>
/// Unit tests for the GetPlansQueryHandler covering successful retrieval and not-found scenarios.
/// </summary>
[TestClass]
public sealed class GetPlansQueryHandlerTests
{
    private readonly Mock<IPlanRepository> _planRepoMock = new();
    private readonly GetPlansQueryHandler _handler;

    public GetPlansQueryHandlerTests()
    {
        _handler = new GetPlansQueryHandler(_planRepoMock.Object);
    }

    // ===== Handle =====

    /// <summary>
    /// Verifies that Handle with existing plans returns a list of PlanResponse objects.
    /// </summary>
    [TestMethod]
    public async Task Handle_PlansExist_ReturnsPlanResponses()
    {
        // Arrange
        Plan plan1 = CreateTestPlan("Plan A", 49.99m);
        Plan plan2 = CreateTestPlan("Plan B", 99.99m);
        var plans = new List<Plan> { plan1, plan2 };

        _planRepoMock.Setup(r => r.GetAllAsNoTrackingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(plans);

        // Act
        Result<List<PlanResponse>> result = await _handler.Handle(new GetPlansQuery(), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(2, result.Value.Count);
    }

    /// <summary>
    /// Verifies that Handle returns correct plan properties for each plan.
    /// </summary>
    [TestMethod]
    public async Task Handle_PlansExist_ReturnsCorrectProperties()
    {
        // Arrange
        Plan plan = CreateTestPlan("Premium Plan", 199.99m);
        var plans = new List<Plan> { plan };

        _planRepoMock.Setup(r => r.GetAllAsNoTrackingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(plans);

        // Act
        Result<List<PlanResponse>> result = await _handler.Handle(new GetPlansQuery(), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        PlanResponse response = result.Value[0];
        Assert.AreEqual(plan.Id, response.Id);
        Assert.AreEqual("Premium Plan", response.Name);
        Assert.AreEqual("Test Description", response.Description);
        Assert.AreEqual(199.99m, response.Price.amount);
        Assert.AreEqual("USD", response.Price.currency);
        Assert.AreEqual("Monthly", response.PlanPeriodTime);
    }

    /// <summary>
    /// Verifies that Handle with plans containing features maps them correctly.
    /// </summary>
    [TestMethod]
    public async Task Handle_PlansWithFeatures_ReturnsMappedFeatures()
    {
        // Arrange
        Plan plan = CreateTestPlan("Plan A", 49.99m);
        Feature feature = Feature.Create(Guid.NewGuid(), "chat", "Chat functionality");
        PlanFeature planFeature = PlanFeature.Create(Guid.NewGuid(), plan.Id, feature.Id);
        SetPlanFeatureNavigation(planFeature, feature);
        SetPlanFeatures(plan, new List<PlanFeature> { planFeature });
        var plans = new List<Plan> { plan };

        _planRepoMock.Setup(r => r.GetAllAsNoTrackingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(plans);

        // Act
        Result<List<PlanResponse>> result = await _handler.Handle(new GetPlansQuery(), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.Value[0].Features.Count);
        Assert.AreEqual("chat", result.Value[0].Features[0].Key);
    }

    /// <summary>
    /// Verifies that Handle with plans containing limited features maps them correctly.
    /// </summary>
    [TestMethod]
    public async Task Handle_PlansWithLimitedFeatures_ReturnsMappedLimitedFeatures()
    {
        // Arrange
        Plan plan = CreateTestPlan("Plan A", 49.99m);
        LimitedFeature limitedFeature = LimitedFeature.MarketingAiPosts;
        PlanLimitedFeature planLimitedFeature = PlanLimitedFeature.Create(Guid.NewGuid(), plan.Id, limitedFeature.Id, 25);
        SetPlanLimitedFeatureNavigation(planLimitedFeature, limitedFeature);
        SetPlanLimitedFeatures(plan, new List<PlanLimitedFeature> { planLimitedFeature });
        var plans = new List<Plan> { plan };

        _planRepoMock.Setup(r => r.GetAllAsNoTrackingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(plans);

        // Act
        Result<List<PlanResponse>> result = await _handler.Handle(new GetPlansQuery(), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.Value[0].LimitedFeatures.Count);
        Assert.AreEqual("marketing_ai_posts", result.Value[0].LimitedFeatures[0].Key);
        Assert.AreEqual(25, result.Value[0].LimitedFeatures[0].Limit);
    }

    /// <summary>
    /// Verifies that Handle with no plans throws NotFoundException.
    /// </summary>
    [TestMethod]
    public async Task Handle_NoPlans_ThrowsNotFoundException()
    {
        // Arrange
        _planRepoMock.Setup(r => r.GetAllAsNoTrackingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Plan>());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new GetPlansQuery(), CancellationToken.None));
    }

    /// <summary>
    /// Verifies that Handle returns plans with empty features and limited features when none exist.
    /// </summary>
    [TestMethod]
    public async Task Handle_PlansWithNoFeatures_ReturnsEmptyCollections()
    {
        // Arrange
        Plan plan = CreateTestPlan("Plan A", 49.99m);
        var plans = new List<Plan> { plan };

        _planRepoMock.Setup(r => r.GetAllAsNoTrackingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(plans);

        // Act
        Result<List<PlanResponse>> result = await _handler.Handle(new GetPlansQuery(), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0, result.Value[0].Features.Count);
        Assert.AreEqual(0, result.Value[0].LimitedFeatures.Count);
    }

    /// <summary>
    /// Verifies that Handle passes the cancellation token to the repository.
    /// </summary>
    [TestMethod]
    public async Task Handle_CallsRepositoryWithCancellationToken()
    {
        // Arrange
        var plans = new List<Plan> { CreateTestPlan("A", 10m) };
        var cts = new CancellationTokenSource();

        _planRepoMock.Setup(r => r.GetAllAsNoTrackingAsync(cts.Token))
            .ReturnsAsync(plans);

        // Act
        await _handler.Handle(new GetPlansQuery(), cts.Token);

        // Assert
        _planRepoMock.Verify(r => r.GetAllAsNoTrackingAsync(cts.Token), Times.Once);
    }

    /// <summary>
    /// Verifies that Handle returns multiple plans each with their own features.
    /// </summary>
    [TestMethod]
    public async Task Handle_MultiplePlansEachWithFeatures_ReturnsCorrectly()
    {
        // Arrange
        Plan plan1 = CreateTestPlan("Plan A", 49.99m);
        Plan plan2 = CreateTestPlan("Plan B", 99.99m);

        Feature f1 = Feature.Create(Guid.NewGuid(), "feature_a", "Feature A");
        Feature f2 = Feature.Create(Guid.NewGuid(), "feature_b", "Feature B");
        PlanFeature pf1 = PlanFeature.Create(Guid.NewGuid(), plan1.Id, f1.Id);
        PlanFeature pf2 = PlanFeature.Create(Guid.NewGuid(), plan2.Id, f2.Id);
        SetPlanFeatureNavigation(pf1, f1);
        SetPlanFeatureNavigation(pf2, f2);
        SetPlanFeatures(plan1, new List<PlanFeature> { pf1 });
        SetPlanFeatures(plan2, new List<PlanFeature> { pf2 });

        var plans = new List<Plan> { plan1, plan2 };

        _planRepoMock.Setup(r => r.GetAllAsNoTrackingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(plans);

        // Act
        Result<List<PlanResponse>> result = await _handler.Handle(new GetPlansQuery(), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.Value[0].Features.Count);
        Assert.AreEqual("feature_a", result.Value[0].Features[0].Key);
        Assert.AreEqual(1, result.Value[1].Features.Count);
        Assert.AreEqual("feature_b", result.Value[1].Features[0].Key);
    }

    // ===== Helpers =====

    private static Plan CreateTestPlan(string name, decimal price)
    {
        return Plan.Create(
            Guid.NewGuid(),
            name,
            "Test Description",
            "Test Content",
            price,
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
