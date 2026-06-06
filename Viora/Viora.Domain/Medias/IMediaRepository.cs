namespace Viora.Domain.Medias;

public interface IMediaRepository
{
    void Add(MediaFile media);
    Task<MediaFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<MediaFile>> GetByIdsAsync(List<Guid> Ids, CancellationToken cancellation);
}
