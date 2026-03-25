using Viora.Domain.Abstractions;


namespace Viora.Infrastructure.Repositories;

internal abstract class Repository<T>(ApplicationDbContext dbContext)
    where T : Entity
{
    protected readonly ApplicationDbContext DbContext = dbContext;

    // consider adding another member to retrieve entities with out tracking
    // for the read queries resulting in better memory management and performance

    #region query ops
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<T>().FindAsync([id], cancellationToken);
    }
    #endregion  

    #region Addition ops
    public void AddRange(IEnumerable<T> entities)
    {
        DbContext.AddRange(entities);
    }

    public virtual void Add(T entity)
    {
        DbContext.Add(entity);
    }
    #endregion

    #region Deletion ops
    public void Remove(Guid id)
    {
        DbContext.Set<T>()
           .Where(entity => entity.Id == id)
           .ExecuteDelete();
    }

    public void Remove(T entity)
    {
        DbContext.Set<T>().Remove(entity);
    }

    public void RemoveRange(IEnumerable<T> entites)
    {
        DbContext.Set<T>().RemoveRange(entites);
    }

    public void RemoveRange(IEnumerable<Guid> ids)
    {
        DbContext.Set<T>()
           .Where(entity => ids.Contains(entity.Id))
           .ExecuteDelete();
    }
    #endregion 
}