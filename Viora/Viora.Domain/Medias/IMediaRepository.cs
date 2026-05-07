namespace Viora.Domain.Medias;

public interface IMediaRepository
{
    public Task<MediaFile> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    public Task<List<MediaFile>> GetByIdsAsync(List<Guid> Ids, CancellationToken cancellation);
}
