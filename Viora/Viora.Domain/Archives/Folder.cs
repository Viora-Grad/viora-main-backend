using Viora.Domain.Abstractions;
using Viora.Domain.Archives.Internals;

namespace Viora.Domain.Archives;

public class Folder : Entity
{
    public Guid ArchiveId { get; private set; }

    public Guid? ParentFolderId { get; private set; }

    public FolderName Name { get; private set; }

    public FolderDescription Description { get; private set; }

    public FolderType Type { get; private set; }

    public int Order { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTime CreatedAt { get; private set; }


    protected Folder() { }

    private Folder(
        Guid id,
        Guid archiveId,
        Guid? parentFolderId,
        FolderName name,
        FolderDescription description,
        FolderType type,
        int order,
        bool isDeleted,
        DateTime createdAt) : base(id)
    {
        ArchiveId = archiveId;
        ParentFolderId = parentFolderId;
        Name = name;
        Description = description;
        Type = type;
        Order = order;
        IsDeleted = isDeleted;
        CreatedAt = createdAt;
    }

    public void Update(FolderName name, FolderDescription description, int order)
    {
        Name = name;
        Description = description;
        Order = order;
    }

    public static Folder Create(
        Guid archiveId,
        Guid? parentFolderId,
        FolderName name,
        FolderDescription description,
        FolderType type,
        int order,
        DateTime createdAt)
    {
        return new Folder(
            Guid.NewGuid(),
            archiveId,
            parentFolderId,
            name,
            description,
            type,
            order,
            false,
            createdAt);
    }
}
