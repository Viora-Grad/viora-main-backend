namespace Viora.Application.Medias.Internals;

public interface IMediaRepository
{
    public Task<MediaFile> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    public IQueryable<MediaFile> GetByIds(IEnumerable<Guid> Ids);
}
