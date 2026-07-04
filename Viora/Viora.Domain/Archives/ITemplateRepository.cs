namespace Viora.Domain.Archives;

public interface ITemplateRepository
{
    Task<Template?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Template>> GetByArchiveIdAsync(Guid archiveId, CancellationToken cancellationToken = default);
    Task<List<Template>> GetByFolderIdAsync(Guid folderId, CancellationToken cancellationToken = default);
    void Add(Template template);
    void Update(Template template);
    void Remove(Template template);
}
