namespace Viora.Domain.Medias;

public interface IMediaRepository
{
    public Task<MediaFile> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    public IQueryable<MediaFile> GetByIds(ICollection<Guid> Ids);
}
