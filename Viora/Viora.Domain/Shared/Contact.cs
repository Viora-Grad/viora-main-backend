using Viora.Domain.Abstractions;
using Viora.Domain.Shared.Internal;

namespace Viora.Domain.Shared;

// Implementing Entity might not be beneficial if Contact is just a value object belonging to another entity, but we follow class diagram for now
public class Contact : Entity
{
    private Contact(Guid id, Email[] emails, PhoneNumber[] phoneNumbers)
        : base(id)
    {
        _emails = emails;
        _phoneNumbers = phoneNumbers;

    }
    private Contact() { } // for ef core
    private ICollection<Email> _emails { get; init; } = [];
    private ICollection<PhoneNumber> _phoneNumbers { get; init; } = [];

    public IReadOnlyCollection<Email> Emails => _emails.ToList().AsReadOnly();
    public IReadOnlyCollection<PhoneNumber> PhoneNumbers => _phoneNumbers.ToList().AsReadOnly();

    public static Contact Create(Guid id, Email[] emails, PhoneNumber[] phoneNumbers)
    {
        return new Contact(id, emails, phoneNumbers);

    }
}
