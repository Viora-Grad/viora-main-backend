using Microsoft.EntityFrameworkCore;
using Viora.Domain.Abstractions;

namespace Viora.Infrastructure.Presistance;

public static class SpecificationEvaluator<T> where T : class
{
    /// <summary>
    /// Builds the full query — criteria + includes + ordering + paging.
    /// Used for ListAsync / FirstOrDefaultAsync.
    /// </summary>
    public static IQueryable<T> GetQuery(IQueryable<T> input, ISpecification<T> spec)
    {
        var query = ApplyCriteria(input, spec);

        query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));

        if (spec.OrderByClauses.Count > 0)
        {
            var first = spec.OrderByClauses[0];
            var ordered = first.Descending
                ? query.OrderByDescending(first.KeySelector)
                : query.OrderBy(first.KeySelector);

            foreach (var clause in spec.OrderByClauses.Skip(1))
            {
                ordered = clause.Descending
                    ? ordered.ThenByDescending(clause.KeySelector)
                    : ordered.ThenBy(clause.KeySelector);
            }

            query = ordered;
        }

        if (spec.IsPagingEnabled && spec.Skip is not null && spec.Take is not null)
            query = query.Skip((int)spec.Skip).Take((int)spec.Take);

        return query;
    }

    /// <summary>
    /// Builds a count-only query — criteria only, no includes/ordering/paging.
    /// Used for CountAsync to get accurate totals.
    /// </summary>
    public static IQueryable<T> GetQueryForCount(IQueryable<T> input, ISpecification<T> spec)
        => ApplyCriteria(input, spec);

    private static IQueryable<T> ApplyCriteria(IQueryable<T> input, ISpecification<T> spec)
        => spec.Criteria is null ? input : input.Where(spec.Criteria);
}