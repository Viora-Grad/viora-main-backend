using Viora.Domain.Abstractions;
using Viora.Domain.MedicalRecords;
using Viora.Domain.Organizations.OrganizationHistory;
using Viora.Domain.Shared;
using Viora.Domain.Users.Identity;
using Viora.Domain.Users.Internal;

namespace Viora.Domain.Users.Customers;
/// <summary>
/// <strong>Resolving User instances to Customer in first assignment
/// might allow Customer and Owner existence for the same UserId which might not be a bad resolution</strong>
/// </summary>

public sealed class Customer : Entity
{
    private readonly HashSet<Guid> _organizationsVisited = [];
    private readonly HashSet<Contact> _contacts = [];
    public UserName? UserName { get; private set; }
    public PersonalInfo PersonalInfo { get; private set; } = null!;
    public DateTime JoinedAt { get; private set; }
    public Guid? MedicalRecordId { get; private set; } // can be removed since the relation is optional from the customer side
    public IReadOnlyList<Guid> OrganizationsVisited => _organizationsVisited.ToList().AsReadOnly();
    public IReadOnlyList<Contact> Contacts => _contacts.ToList().AsReadOnly();
    public AuthAccount AuthAccount { get; private set; } = null!; // navigation property for ef core
    public MedicalRecord? MedicalRecord { get; private set; } // navigation property for ef core
    public ICollection<OrganizationVisits> OrganizationVisits { get; private set; } = null!; // navigation property for ef core
    private Customer() { } // for ef core
    private Customer(Guid id, UserName? userName, PersonalInfo personalInfo, DateTime joinedAt, Guid? medicalRecordId)
        : base(id)
    {
        UserName = userName;
        PersonalInfo = personalInfo;
        JoinedAt = joinedAt;
        MedicalRecordId = medicalRecordId;
    }
    public static Customer Create(UserName? userName, PersonalInfo personalInfo, DateTime utcNow, Guid? medicalRecordId)
    {
        // add any validation if needed
        return new Customer(Guid.NewGuid(), userName, personalInfo, utcNow, medicalRecordId);
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
