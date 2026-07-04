namespace Viora.Domain.Archives;

public interface IFolderRepository
{
    Task<Folder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Folder>> GetByArchiveIdAsync(Guid archiveId, CancellationToken cancellationToken = default);
    Task<List<Folder>> GetByParentFolderIdAsync(Guid parentFolderId, CancellationToken cancellationToken = default);
    Task<List<Folder>> GetRootFoldersAsync(Guid archiveId, CancellationToken cancellationToken = default);
    void Add(Folder folder);
    void Update(Folder folder);
    void Remove(Folder folder);
}
