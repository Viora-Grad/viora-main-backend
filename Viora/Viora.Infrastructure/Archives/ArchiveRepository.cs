using MongoDB.Driver;
using Viora.Domain.Archives;

namespace Viora.Infrastructure.Archives;

internal class ArchiveRepository : IArchiveRepository
{
    private readonly MongoDbContext _context;

    public ArchiveRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Archive?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Archives
            .Find(a => a.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Archive>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _context.Archives
            .Find(a => a.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);
    }

    public void Add(Archive archive)
    {
        _context.Archives.InsertOne(archive);
    }

    public void Update(Archive archive)
    {
        _context.Archives.ReplaceOne(a => a.Id == archive.Id, archive);
    }

    public void Remove(Archive archive)
    {
        _context.Archives.DeleteOne(a => a.Id == archive.Id);
    }
}
