using MongoDB.Driver;
using Viora.Domain.Archives;

namespace Viora.Infrastructure.Archives;

internal class FolderRepository : IFolderRepository
{
    private readonly MongoDbContext _context;

    public FolderRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Folder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Folders
            .Find(f => f.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Folder>> GetByArchiveIdAsync(Guid archiveId, CancellationToken cancellationToken = default)
    {
        return await _context.Folders
            .Find(f => f.ArchiveId == archiveId && !f.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Folder>> GetByParentFolderIdAsync(Guid parentFolderId, CancellationToken cancellationToken = default)
    {
        return await _context.Folders
            .Find(f => f.ParentFolderId == parentFolderId && !f.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Folder>> GetRootFoldersAsync(Guid archiveId, CancellationToken cancellationToken = default)
    {
        return await _context.Folders
            .Find(f => f.ArchiveId == archiveId && f.ParentFolderId == null && !f.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public void Add(Folder folder)
    {
        _context.Folders.InsertOne(folder);
    }

    public void Update(Folder folder)
    {
        _context.Folders.ReplaceOne(f => f.Id == folder.Id, folder);
    }

    public void Remove(Folder folder)
    {
        _context.Folders.DeleteOne(f => f.Id == folder.Id);
    }
}
