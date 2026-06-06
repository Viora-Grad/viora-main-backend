using Viora.Domain.Abstractions;

namespace Viora.Domain.Branches;

public static class BranchErrors
{
    public static readonly Error InvalidOpenTimeInterval =
        new("Branches.InvalidOpenTimeInterval", "Open time must be between 00:00 and 23:59", ErrorCategory.Validation);

    public static readonly Error InvalidCloseTimeInterval =
        new("Branches.InvalidCloseTimeInterval", "Close time must be between 00:00 and 23:59", ErrorCategory.Validation);

    public static readonly Error OpenTimeAfterCloseTime =
        new("Branches.OpenTimeAfterCloseTime", "Close time must be after open time", ErrorCategory.Conflict);

}
