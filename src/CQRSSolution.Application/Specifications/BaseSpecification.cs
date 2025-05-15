using System.Linq.Expressions;
using CQRSSolution.Application.Interfaces;

namespace CQRSSolution.Application.Specifications;

/// <summary>
///     Provides a base implementation for <see cref="ISpecification{T}" />.
///     Derived classes can use protected methods to configure query criteria,
///     eager loading (includes), ordering, and pagination.
/// </summary>
/// <typeparam name="T">The type of the entity this specification applies to.</typeparam>
public abstract class BaseSpecification<T> : ISpecification<T>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BaseSpecification{T}" /> class.
    /// </summary>
    protected BaseSpecification()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BaseSpecification{T}" /> class
    ///     with the specified criteria.
    /// </summary>
    /// <param name="criteria">The criteria expression to filter entities.</param>
    protected BaseSpecification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    /// <inheritdoc />
    public Expression<Func<T, bool>>? Criteria { get; protected set; }

    /// <inheritdoc />
    public List<Expression<Func<T, object>>> Includes { get; } = new();

    /// <inheritdoc />
    public List<string> IncludeStrings { get; } = new();

    /// <inheritdoc />
    public Expression<Func<T, object>>? OrderBy { get; private set; }

    /// <inheritdoc />
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }

    /// <inheritdoc />
    public int Take { get; private set; }

    /// <inheritdoc />
    public int Skip { get; private set; }

    /// <inheritdoc />
    public bool IsPagingEnabled { get; private set; }

    /// <summary>
    ///     Adds an include expression for eager loading a related entity or collection.
    /// </summary>
    /// <param name="includeExpression">The expression representing the navigation property to include.</param>
    protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    /// <summary>
    ///     Adds an include string for eager loading related entities using a string path.
    ///     This is typically used for more complex include paths not easily represented by expressions.
    /// </summary>
    /// <param name="includeString">The string representing the navigation path to include (e.g., "Order.OrderItems").</param>
    protected virtual void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    /// <summary>
    ///     Applies ordering in ascending order based on the specified expression.
    /// </summary>
    /// <param name="orderByExpression">The expression representing the property to order by.</param>
    protected virtual void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
        OrderByDescending = null; // Clear descending order if ascending is set
    }

    /// <summary>
    ///     Applies ordering in descending order based on the specified expression.
    /// </summary>
    /// <param name="orderByDescendingExpression">The expression representing the property to order by in descending order.</param>
    protected virtual void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
    {
        OrderByDescending = orderByDescendingExpression;
        OrderBy = null; // Clear ascending order if descending is set
    }

    /// <summary>
    ///     Applies pagination to the query.
    /// </summary>
    /// <param name="skip">The number of items to skip (page number * page size).</param>
    /// <param name="take">The number of items to take (page size).</param>
    protected virtual void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }
}