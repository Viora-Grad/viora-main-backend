using Viora.Domain.Abstractions;
using Viora.Domain.Archives.Internals;

namespace Viora.Domain.Archives;

public class Record : Entity
{
    public Guid ArchiveId { get; private set; }

    public Guid FolderId { get; private set; }

    public Guid CustomerId { get; private set; }

    public Guid? AppointmentId { get; private set; }

    public Guid TemplateId { get; private set; }

    public Guid TemplateVersionId { get; private set; }

    private List<RecordFieldValue> _values = [];

    public IReadOnlyCollection<RecordFieldValue> Values => _values;

    private List<Attachment> _attachments = [];

    public IReadOnlyCollection<Attachment> Attachments => _attachments;

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }


    protected Record() { }

    private Record(
        Guid id,
        Guid archiveId,
        Guid folderId,
        Guid customerId,
        Guid? appointmentId,
        Guid templateId,
        Guid templateVersionId,
        DateTime createdAt,
        DateTime? updatedAt) : base(id)
    {
        ArchiveId = archiveId;
        FolderId = folderId;
        CustomerId = customerId;
        AppointmentId = appointmentId;
        TemplateId = templateId;
        TemplateVersionId = templateVersionId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static Record Create(
        Guid archiveId,
        Guid folderId,
        Guid customerId,
        Guid? appointmentId,
        Guid templateId,
        Guid templateVersionId,
        DateTime createdAt)
    {
        return new Record(
            Guid.NewGuid(),
            archiveId,
            folderId,
            customerId,
            appointmentId,
            templateId,
            templateVersionId,
            createdAt,
            null);
    }

    public void AddValue(RecordFieldValue value)
    {
        _values.Add(value);
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearValues()
    {
        _values.Clear();
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddAttachment(Attachment attachment)
    {
        _attachments.Add(attachment);
        UpdatedAt = DateTime.UtcNow;
    }
}
