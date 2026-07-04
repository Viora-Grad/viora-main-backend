using Viora.Domain.Abstractions;
using Viora.Domain.Archives.Internals;

namespace Viora.Domain.Archives;

public class Template : Entity
{
    public Guid ArchiveId { get; private set; }

    public Guid FolderId { get; private set; }

    public TemplateName Name { get; private set; }

    public TemplateDescription Description { get; private set; }

    public int CurrentVersion { get; private set; }

    private List<TemplateVersion> _versions = [];

    public IReadOnlyCollection<TemplateVersion> Versions => _versions;

    public DateTime CreatedAt { get; private set; }


    protected Template() { }

    private Template(
        Guid id,
        Guid archiveId,
        Guid folderId,
        TemplateName name,
        TemplateDescription description,
        int currentVersion,
        DateTime createdAt) : base(id)
    {
        ArchiveId = archiveId;
        FolderId = folderId;
        Name = name;
        Description = description;
        CurrentVersion = currentVersion;
        CreatedAt = createdAt;
    }

    public void Update(TemplateName name, TemplateDescription description)
    {
        Name = name;
        Description = description;
    }

    public static Template Create(
        Guid archiveId,
        Guid folderId,
        TemplateName name,
        TemplateDescription description,
        DateTime createdAt)
    {
        return new Template(
            Guid.NewGuid(),
            archiveId,
            folderId,
            name,
            description,
            0,
            createdAt);
    }

    public void AddVersion(TemplateVersion version)
    {
        _versions.Add(version);
        CurrentVersion = version.Version;
    }
}
