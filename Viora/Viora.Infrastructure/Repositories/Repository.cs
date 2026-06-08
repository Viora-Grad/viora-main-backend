using Microsoft.EntityFrameworkCore;
using Viora.Domain.Abstractions;
using Viora.Infrastructure.Presistance;


namespace Viora.Infrastructure.Repositories;

internal abstract class Repository<T>(ApplicationDbContext dbContext)
    where T : Entity
{
    protected readonly ApplicationDbContext DbContext = dbContext;

    // consider adding another member to retrieve entities with out tracking
    // for the read queries resulting in better memory management and performance

    #region query ops
    // marked virtual to allow overriding for better performance in some cases.
    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<T>().FindAsync([id], cancellationToken);
    }
    public IQueryable<T> GetByIds(IEnumerable<Guid> ids)
    {
        return DbContext.Set<T>().Where(entity => ids.Contains(entity.Id));
    }
    public async Task<List<T>> GetAllAsNoTrackingAsync(CancellationToken cancellationToken)
    {
        return await DbContext.Set<T>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<T>> GetByIdsAsync(List<Guid> ids, CancellationToken cancellationToken)
    {
        if (ids is null || ids.Count == 0)
            return [];

        var idList = ids.Distinct().ToList();

        return await DbContext.Set<T>()
            .Where(e => idList.Contains(EF.Property<Guid>(e, "Id")))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await DbContext.Set<T>().AnyAsync(x => x.Id == id, cancellationToken);
    }
    public async Task<long> CountAsync(
      ISpecification<T> spec,
      CancellationToken cancellationToken = default)
    {
        return await SpecificationEvaluator<T>
            .GetQueryForCount(DbContext.Set<T>().AsQueryable(), spec)
            .LongCountAsync(cancellationToken);
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