namespace Viora.Domain.Archives;

public interface IRecordRepository
{
    Task<Record?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Record>> GetByFolderIdAsync(Guid folderId, CancellationToken cancellationToken = default);
    Task<List<Record>> GetByArchiveIdAsync(Guid archiveId, CancellationToken cancellationToken = default);
    Task<List<Record>> SearchAsync(Guid archiveId, string? searchTerm, Guid? folderId, DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default);
    void Add(Record record);
    void Update(Record record);
    void Remove(Record record);
}
