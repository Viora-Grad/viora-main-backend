using Viora.Domain.Abstractions;
using Viora.Domain.MedicalRecords;
using Viora.Domain.Organizations.OrganizationHistory;
using Viora.Domain.Shared.Internal;
using Viora.Domain.Users.Identity;
using Viora.Domain.Users.Internal;
using Email = Viora.Domain.Users.Internal.Email;

namespace Viora.Domain.Users.Customers;
/// <summary>
/// <strong>Resolving User instances to Customer in first assignment
/// might allow Customer and Owner existence for the same UserId which might not be a bad resolution</strong>
/// </summary>

public sealed class Customer : Entity
{
    private readonly HashSet<Guid> _organizationsVisited = [];
    private readonly HashSet<PhoneNumber> _phoneNumbers = [];
    private readonly HashSet<Email> _contactEmails = [];
    public UserName? UserName { get; private set; }
    public PersonalInfo PersonalInfo { get; private set; } = null!;
    public DateTime JoinedAt { get; private set; }
    public Guid? MedicalRecordId { get; private set; } // can be removed since the relation is optional from the customer side
    public Guid? ProfilePicId { get; private set; }
    public IReadOnlyList<Guid> OrganizationsVisited => _organizationsVisited.ToList().AsReadOnly();
    public IReadOnlyCollection<PhoneNumber> PhoneNumbers => _phoneNumbers;
    public IReadOnlyCollection<Email> Emails => _contactEmails.ToList().AsReadOnly();
    public User UserProfile { get; private set; } = null!; // navigation property for ef core
    public MedicalRecord? MedicalRecord { get; private set; } // navigation property for ef core
    public ICollection<OrganizationVisits> OrganizationVisits { get; private set; } = null!; // navigation property for ef core
    private Customer() { } // for ef core
    private Customer(Guid id,
        UserName? userName,
        PersonalInfo personalInfo,
        DateTime joinedAt,
        IEnumerable<PhoneNumber>? phoneNumbers,
        IEnumerable<Email>? contactEmails)
        : base(id)
    {
        UserName = userName;
        PersonalInfo = personalInfo;
        JoinedAt = joinedAt;

        if (phoneNumbers is not null)
        {
            _phoneNumbers.UnionWith(phoneNumbers);
        }
        if (contactEmails is not null)
        {
            _contactEmails.UnionWith(contactEmails);
        }
    }
    public static Customer Create(Guid id,
        UserName? userName,
        PersonalInfo personalInfo,
        DateTime utcNow,
        IEnumerable<PhoneNumber>? phoneNumbers,
        IEnumerable<Email>? contactEmails)
    {
        // add any validation if needed
        return new Customer(id, userName, personalInfo, utcNow, phoneNumbers, contactEmails);
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
    public Result AddPhoneNumbers(PhoneNumber[] phoneNumbers)
    {
        _phoneNumbers.UnionWith(phoneNumbers);
        return Result.Success();
    }
    public Result AddContactEmails(Email[] contactEmails)
    {
        _contactEmails.UnionWith(contactEmails);
        return Result.Success();
    }
    public Result UpdateProfilePicture(Guid profilePicId)
    {
        ProfilePicId = profilePicId;
        return Result.Success();
    }
}
