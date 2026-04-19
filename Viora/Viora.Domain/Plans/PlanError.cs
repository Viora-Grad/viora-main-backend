using Viora.Domain.Abstractions;

namespace Viora.Domain.Plans;

public static class PlanError
{
    public readonly static Error NotValid = new Error("notValidInput", "Plan is not valid.", ErrorCategory.Validation);
    public readonly static Error InvalidPlanPeriod = new Error("invalidPlanPeriod", "The plan period is invalid.", ErrorCategory.Validation);
    public readonly static Error InvalidPlanFeature = new Error("invalidPlanFeature", "The plan feature is invalid.", ErrorCategory.Validation);
}
