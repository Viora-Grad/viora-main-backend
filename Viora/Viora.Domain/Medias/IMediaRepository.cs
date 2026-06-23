namespace Viora.Domain.Medias;

public interface IMediaRepository
{
    // TODO over ride the add method to get the org Id and if not null consume from the quota of the org
    void Add(MediaFile media);
    void AddRange(IEnumerable<MediaFile> medias);
    Task<MediaFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<MediaFile>> GetByIdsAsync(List<Guid> Ids, CancellationToken cancellation);
}
