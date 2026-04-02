using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.Suspensions.Internals;

namespace Viora.Domain.Organizations.Suspensions;

public sealed class Suspension : Entity
{
    public Guid? OrganizationId { get; private set; }
    public Guid OwnerId { get; private set; }
    public OrganizationName OrganizationName { get; private set; } = default!;

    public Guid? SuspendedById { get; private set; }
    public SuspensionReason Reason { get; private set; }
    public SuspensionSource Source { get; private set; }
    public SuspensionNote Note { get; private set; } = default!;

    public DateOnly SuspensionDateUtc { get; private set; }
    public DateOnly ScheduledDeletionDateUtc { get; private set; }

    private Suspension() { } // for EfCore

    /// <summary>
    /// takes the current date time to check if an org is already deleted or not,
    /// if the deletion date is less than or equal to the current date, then it is considered deleted.
    /// </summary>
    /// <returns></returns>
    public Result<bool> IsDeletedAt(DateOnly referencedate) => Result.Success(ScheduledDeletionDateUtc <= referencedate);
}
