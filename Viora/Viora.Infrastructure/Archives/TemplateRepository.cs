using MongoDB.Driver;
using Viora.Domain.Archives;

namespace Viora.Infrastructure.Archives;

internal class TemplateRepository : ITemplateRepository
{
    private readonly MongoDbContext _context;

    public TemplateRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Template?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Templates
            .Find(t => t.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Template>> GetByArchiveIdAsync(Guid archiveId, CancellationToken cancellationToken = default)
    {
        return await _context.Templates
            .Find(t => t.ArchiveId == archiveId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Template>> GetByFolderIdAsync(Guid folderId, CancellationToken cancellationToken = default)
    {
        return await _context.Templates
            .Find(t => t.FolderId == folderId)
            .ToListAsync(cancellationToken);
    }

    public void Add(Template template)
    {
        _context.Templates.InsertOne(template);
    }

    public void Update(Template template)
    {
        _context.Templates.ReplaceOne(t => t.Id == template.Id, template);
    }

    public void Remove(Template template)
    {
        _context.Templates.DeleteOne(t => t.Id == template.Id);
    }
}
