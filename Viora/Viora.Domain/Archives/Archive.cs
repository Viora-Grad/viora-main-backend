using Viora.Domain.Abstractions;
using Viora.Domain.Archives.Internals;

namespace Viora.Domain.Archives;

public class Archive : Entity
{
    public Guid OrganizationId { get; private set; }
    public ArchiveName Name { get; private set; }
    public ArchiveDescription Description { get; private set; }
    public Guid RootFolder { get; private set; }
    public ArchiveSettings Setting { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected Archive() { }


    private Archive(Guid id, Guid organizationId, ArchiveName name, ArchiveDescription description, Guid rootFolder, ArchiveSettings setting, DateTime createdAt) : base(id)
    {
        OrganizationId = organizationId;
        Name = name;
        Description = description;
        RootFolder = rootFolder;
        Setting = setting;
        CreatedAt = createdAt;
    }

    public void Update(ArchiveName name, ArchiveDescription description, ArchiveSettings setting)
    {
        Name = name;
        Description = description;
        Setting = setting;
    }

    public static Archive Create(
        Guid organizationId,
        ArchiveName name,
        ArchiveDescription description,
        ArchiveSettings setting,
        DateTime createdAt)
    {
        var archive = new Archive(
            Guid.NewGuid(),
            organizationId,
            name,
            description,
            Guid.NewGuid(),
            setting,
            createdAt);

        return archive;
    }
}
