using Viora.Domain.Abstractions;
using Viora.Domain.MedicalRecords;

namespace Viora.Domain.Users;
/// <summary>
/// <strong>Resolving User instances to Customer in first assignment
/// might allow Customer and Owner existence for the same UserId which might not be a bad resolution</strong>
/// </summary>

public sealed class Customer : Entity
{
    private readonly HashSet<Guid> _organizationsVisited = [];
    public Guid UserId { get; private set; } // required and Unique
    public Guid? MedicalRecordId { get; private set; } // can be removed since the relation is optional from the customer side
    public IReadOnlyList<Guid> OrganizationsVisited => _organizationsVisited.ToList().AsReadOnly();
    public User User { get; private set; } = null!; // navigation property for ef core
    public MedicalRecord? MedicalRecord { get; private set; } // navigation property for ef core
    private Customer() { } // for ef core
    private Customer(Guid id, Guid userId, Guid? medicalRecordId)
        : base(id)
    {
        UserId = userId;
        MedicalRecordId = medicalRecordId;
    }
    public static Customer Create(Guid userId, Guid? medicalRecordId)
    {
        // add any validation if needed
        return new Customer(Guid.NewGuid(), userId, medicalRecordId);
    }
    public Result AddMedicalRecord(Guid medicalRecordId)
    {
        // might trigger domain events for medical record change
        if (MedicalRecordId is null)
            MedicalRecordId = medicalRecordId;
        else
            return Result.Failure(CustomerErrors.MedicalRecordAlreadyExists);
        return Result.Success();
    }

    public Result VisitOrganization(Guid organizationId)
    {
        _organizationsVisited.Add(organizationId);
        return Result.Success();
    }
    public Result RemoveVisitedOrganization(Guid organizationId)
    {
        _organizationsVisited.Remove(organizationId);
        return Result.Success();
    }
    public bool IsOrganizationVisited(Guid organizationId)
    {
        return _organizationsVisited.Contains(organizationId);
    }

}
