using Viora.Domain.Abstractions;

namespace Viora.Domain.Archives;

public class TemplateVersion : Entity
{
    public Guid TemplateId { get; private set; }

    public int Version { get; private set; }

    public bool IsPublished { get; private set; }

    private List<TemplateField> _fields = [];

    public IReadOnlyCollection<TemplateField> Fields => _fields;

    public DateTime CreatedAt { get; private set; }

    protected TemplateVersion() { }

    private TemplateVersion(
        Guid id,
        Guid templateId,
        int version,
        bool isPublished,
        DateTime createdAt) : base(id)
    {
        TemplateId = templateId;
        Version = version;
        IsPublished = isPublished;
        CreatedAt = createdAt;
    }

    public static TemplateVersion Create(
        Guid templateId,
        int version,
        DateTime createdAt)
    {
        return new TemplateVersion(
            Guid.NewGuid(),
            templateId,
            version,
            false,
            createdAt);
    }

    public void Publish()
    {
        IsPublished = true;
    }

    public void AddField(TemplateField field)
    {
        _fields.Add(field);
    }
}
