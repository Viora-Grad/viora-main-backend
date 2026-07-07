using Viora.Domain.Plans;
using Viora.Domain.Plans.Internal;
using Viora.Domain.Shared;

namespace Viora.Test.Compenents.Domain.Plans;

/// <summary>
/// Unit tests for the Plan entity covering the Create factory method and property initialization.
/// </summary>
[TestClass]
public sealed class PlanTests
{
    /// <summary>
    /// Verifies that Create with valid input returns a Plan with all properties correctly assigned.
    /// </summary>
    [TestMethod]
    public void Create_ValidInput_ReturnsPlanWithCorrectProperties()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        string name = "Basic Plan";
        string description = "A basic plan";
        string content = "Plan content here";
        decimal amount = 9.99m;
        Currency currency = Currency.Usd;
        PlanPeriod planPeriod = PlanPeriod.monthly;

        // Act
        Plan plan = Plan.Create(id, name, description, content, amount, currency, planPeriod);

        // Assert
        Assert.IsNotNull(plan);
        Assert.AreEqual(id, plan.Id);
        Assert.AreEqual(name, plan.Name.value);
        Assert.AreEqual(description, plan.Description.Value);
        Assert.AreEqual(content, plan.Content.Value);
        Assert.AreEqual(amount, plan.Price.Amount);
        Assert.AreEqual(currency, plan.Price.Currency);
        Assert.AreEqual(planPeriod, plan.PlanPeriod);
    }

    /// <summary>
    /// Verifies that Create with EGP currency sets the correct currency on the Price.
    /// </summary>
    [TestMethod]
    public void Create_WithDifferentCurrency_SetsCorrectCurrency()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        Plan plan = Plan.Create(id, "Plan", "Desc", "Content", 100m, Currency.Egp, PlanPeriod.annually);

        // Assert
        Assert.AreEqual(Currency.Egp, plan.Price.Currency);
        Assert.AreEqual(100m, plan.Price.Amount);
    }

    /// <summary>
    /// Verifies that PlanFeatures and PlanLimitedFeatures collections are initialized as empty.
    /// </summary>
    [TestMethod]
    public void Create_InitializesEmptyCollections()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        Plan plan = Plan.Create(id, "Plan", "Desc", "Content", 10m, Currency.Usd, PlanPeriod.monthly);

        // Assert
        Assert.IsNotNull(plan.PlanFeatures);
        Assert.AreEqual(0, plan.PlanFeatures.Count);
        Assert.IsNotNull(plan.PlanLimitedFeatures);
        Assert.AreEqual(0, plan.PlanLimitedFeatures.Count);
    }

    /// <summary>
    /// Verifies that a specific GUID provided to Create is used as the Plan's Id.
    /// </summary>
    [TestMethod]
    public void Create_SetsIdCorrectly()
    {
        // Arrange
        Guid specificId = new("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        // Act
        Plan plan = Plan.Create(specificId, "Plan", "Desc", "Content", 50m, Currency.Usd, PlanPeriod.semiAnnually);

        // Assert
        Assert.AreEqual(specificId, plan.Id);
    }

    /// <summary>
    /// Verifies that a Plan can be created with a zero price amount.
    /// </summary>
    [TestMethod]
    public void Create_WithZeroPrice_SetsZeroAmount()
    {
        // Arrange & Act
        Plan plan = Plan.Create(Guid.NewGuid(), "Free Plan", "Desc", "Content", 0m, Currency.Usd, PlanPeriod.monthly);

        // Assert
        Assert.AreEqual(0m, plan.Price.Amount);
    }

    /// <summary>
    /// Verifies that a Plan can be created with a negative price amount.
    /// </summary>
    [TestMethod]
    public void Create_WithNegativePrice_SetsNegativeAmount()
    {
        // Arrange & Act
        Plan plan = Plan.Create(Guid.NewGuid(), "Plan", "Desc", "Content", -5m, Currency.Usd, PlanPeriod.monthly);

        // Assert
        Assert.AreEqual(-5m, plan.Price.Amount);
    }

    /// <summary>
    /// Verifies that Create correctly assigns monthly, annually, and semi-annually plan periods.
    /// </summary>
    [TestMethod]
    public void Create_WithAllPlanPeriods_SetsCorrectPeriod()
    {
        // Arrange & Act
        Plan monthlyPlan = Plan.Create(Guid.NewGuid(), "Monthly", "Desc", "Content", 10m, Currency.Usd, PlanPeriod.monthly);
        Plan annualPlan = Plan.Create(Guid.NewGuid(), "Annual", "Desc", "Content", 100m, Currency.Usd, PlanPeriod.annually);
        Plan semiAnnualPlan = Plan.Create(Guid.NewGuid(), "Semi", "Desc", "Content", 50m, Currency.Usd, PlanPeriod.semiAnnually);

        // Assert
        Assert.AreEqual(PlanPeriod.monthly, monthlyPlan.PlanPeriod);
        Assert.AreEqual(PlanPeriod.annually, annualPlan.PlanPeriod);
        Assert.AreEqual(PlanPeriod.semiAnnually, semiAnnualPlan.PlanPeriod);
    }
}
