using System.Linq.Expressions;

namespace CQRSSolution.Application.Interfaces;

/// <summary>
///     Defines a contract for a query specification.
///     A specification encapsulates query logic such as filtering, ordering, and eager loading.
/// </summary>
/// <typeparam name="T">The type of the entity this specification applies to.</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    ///     Gets the criteria expression used to filter entities.
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>
    ///     Gets the list of include expressions for eager loading related entities.
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    ///     Gets the list of string-based include paths for eager loading.
    ///     Useful for scenarios where expression-based includes are complex or not possible.
    /// </summary>
    List<string> IncludeStrings { get; }

    /// <summary>
    ///     Gets the expression for ordering entities in ascending order.
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>
    ///     Gets the expression for ordering entities in descending order.
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }

    /// <summary>
    ///     Gets the number of items to take for pagination (page size).
    /// </summary>
    int Take { get; }

    /// <summary>
    ///     Gets the number of items to skip for pagination.
    /// </summary>
    int Skip { get; }

    /// <summary>
    ///     Gets a value indicating whether pagination is enabled.
    /// </summary>
    bool IsPagingEnabled { get; }
}