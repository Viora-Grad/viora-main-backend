using Viora.Domain.Abstractions;

namespace Viora.Domain.Plans;

public static class PlanError
{
    public readonly static Error NotValid = new Error("notValidInput", "Plan is not valid.", ErrorCategory.Validation);
}
