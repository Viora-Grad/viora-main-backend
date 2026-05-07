using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.Suspensions.Internals;

namespace Viora.Domain.Organizations.Suspensions;

public sealed class Suspension : Entity
{
    public Guid? OrganizationId { get; private set; }
    public Guid OwnerId { get; private set; }
    public Guid? SuspendedById { get; private set; }

    public OrganizationName OrganizationName { get; private set; } = default!;
    public SuspensionReason Reason { get; private set; }
    public SuspensionSource Source { get; private set; }
    public SuspensionNote Notes { get; private set; } = default!;

    public DateTime SuspensionDateUtc { get; private set; }
    public DateTime ScheduledDeletionDateUtc { get; private set; }

    private Suspension() { } // for EfCore

    public static Result<Suspension> Create(
        Guid organizationId,
        Guid ownerId,
        string organizationName,
        Guid? suspendedById,
        SuspensionReason suspensionReason,
        string notes,
        DateTime currentDateTime,
        ISuspensionSettings suspensionSettings)
    {
        var result = new Suspension()
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            OrganizationId = organizationId,
            OrganizationName = organizationName,
            SuspendedById = suspendedById,
            Reason = suspensionReason,
            Notes = notes,
            SuspensionDateUtc = currentDateTime,
            ScheduledDeletionDateUtc = currentDateTime + suspensionSettings.DeletionSpan,
        };

        if (suspendedById is not null)
            result.Source = SuspensionSource.Admin;
        else
            result.Source = SuspensionSource.System;

        return Result.Success(result);

    }

    /// <summary>
    /// takes the current date time to check if an org is already deleted or not,
    /// if the deletion date is less than or equal to the current date, then it is considered deleted.
    /// </summary>
    /// <returns></returns>
    public Result<bool> IsDeletedAt(DateTime referencedate) => Result.Success(ScheduledDeletionDateUtc <= referencedate);
}
