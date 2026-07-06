using MongoDB.Driver;
using Viora.Domain.Archives;
using Viora.Domain.Archives.Internals;

namespace Viora.Infrastructure.Archives;

internal class RecordRepository : IRecordRepository
{
    private readonly MongoDbContext _context;

    public RecordRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Record?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Records
            .Find(r => r.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Record>> GetByFolderIdAsync(Guid folderId, CancellationToken cancellationToken = default)
    {
        return await _context.Records
            .Find(r => r.FolderId == folderId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Record>> GetByArchiveIdAsync(Guid archiveId, CancellationToken cancellationToken = default)
    {
        return await _context.Records
            .Find(r => r.ArchiveId == archiveId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Record>> SearchAsync(
        Guid archiveId,
        string? searchTerm,
        Guid? folderId,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var filterBuilder = Builders<Record>.Filter;
        var filters = new List<FilterDefinition<Record>>
        {
            filterBuilder.Eq(r => r.ArchiveId, archiveId)
        };

        if (folderId.HasValue)
            filters.Add(filterBuilder.Eq(r => r.FolderId, folderId.Value));

        if (fromDate.HasValue)
            filters.Add(filterBuilder.Gte(r => r.CreatedAt, fromDate.Value));

        if (toDate.HasValue)
            filters.Add(filterBuilder.Lte(r => r.CreatedAt, toDate.Value));

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var elementFilter = Builders<RecordFieldValue>.Filter.Regex(
                v => v.FieldName, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"));
            filters.Add(filterBuilder.ElemMatch("_values", elementFilter));
        }

        var combined = filterBuilder.And(filters);
        return await _context.Records
            .Find(combined)
            .SortByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public void Add(Record record)
    {
        _context.Records.InsertOne(record);
    }

    public void Update(Record record)
    {
        _context.Records.ReplaceOne(r => r.Id == record.Id, record);
    }

    public void Remove(Record record)
    {
        _context.Records.DeleteOne(r => r.Id == record.Id);
    }
}
