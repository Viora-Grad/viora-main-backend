using System.Linq.Expressions;

namespace Viora.Domain.Abstractions;

public record OrderByClause<T>(Expression<Func<T, object>> KeySelector, bool Descending);

public interface ISpecification<T>
{
    Expression<Func<T, bool>>? Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    List<OrderByClause<T>> OrderByClauses { get; }
    int? Take { get; }
    int? Skip { get; }
    bool IsPagingEnabled { get; }
}

public abstract class BaseSpecification<TEntity> : ISpecification<TEntity>
{
    protected BaseSpecification() { }

    protected BaseSpecification(Expression<Func<TEntity, bool>> criteria)
    {
        Criteria = criteria;
    }

    public Expression<Func<TEntity, bool>>? Criteria { get; private set; }
    public List<Expression<Func<TEntity, object>>> Includes { get; } = [];
    public List<OrderByClause<TEntity>> OrderByClauses { get; } = [];
    public int? Take { get; private set; }
    public int? Skip { get; private set; }
    public bool IsPagingEnabled { get; private set; }

    int? ISpecification<TEntity>.Take => Take;

    int? ISpecification<TEntity>.Skip => Skip;

    protected void AddInclude(Expression<Func<TEntity, object>> includeExpression)
        => Includes.Add(includeExpression);

    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }

    protected void ApplyOrderBy(Expression<Func<TEntity, object>> keySelector)
        => OrderByClauses.Add(new OrderByClause<TEntity>(keySelector, Descending: false));

    protected void ApplyOrderByDescending(Expression<Func<TEntity, object>> keySelector)
        => OrderByClauses.Add(new OrderByClause<TEntity>(keySelector, Descending: true));

    protected void AddCriteria(Expression<Func<TEntity, bool>> criteria)
    {
        Criteria = Criteria == null
            ? criteria
            : Criteria.And(criteria);
    }
}

/// <summary>
/// declares how the AND operator should work for combining two expressions of the same type. This is useful for building complex specifications by combining simpler ones.
/// Also declared the extension for optional Or and Not ops
/// </summary>
public static class ExpressionExtensions
{
    public static Expression<Func<T, bool>> And<T>(
        this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        var parameter = Expression.Parameter(typeof(T));
        var leftBody = new ReplaceParameterVisitor(left.Parameters[0], parameter).Visit(left.Body);
        var rightBody = new ReplaceParameterVisitor(right.Parameters[0], parameter).Visit(right.Body);
        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(leftBody!, rightBody!), parameter);
    }

    public static Expression<Func<T, bool>> Or<T>(
    this Expression<Func<T, bool>> left,
    Expression<Func<T, bool>> right)
    {
        var parameter = Expression.Parameter(typeof(T));
        var leftBody = new ReplaceParameterVisitor(left.Parameters[0], parameter).Visit(left.Body);
        var rightBody = new ReplaceParameterVisitor(right.Parameters[0], parameter).Visit(right.Body);
        return Expression.Lambda<Func<T, bool>>(Expression.OrElse(leftBody!, rightBody!), parameter);
    }

    public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> expression)
    {
        var negated = Expression.Not(expression.Body);
        return Expression.Lambda<Func<T, bool>>(negated, expression.Parameters);
    }

    private class ReplaceParameterVisitor(ParameterExpression oldParam, ParameterExpression newParam) : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParam = oldParam;
        private readonly ParameterExpression _newParam = newParam;

        protected override Expression VisitParameter(ParameterExpression node)
            => node == _oldParam ? _newParam : base.VisitParameter(node);
    }
}