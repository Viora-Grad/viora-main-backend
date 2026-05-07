using Viora.Domain.Medias;

namespace Viora.Infrastructure.Repositories;

internal class MediaRepository(ApplicationDbContext dbContext) : Repository<MediaFile>(dbContext), IMediaRepository
{
    public IQueryable<MediaFile> GetByIds(ICollection<Guid> Ids)
    {
        throw new NotImplementedException();
    }
}
