using CQRSSolution.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CQRSSolution.Infrastructure.Persistence;

/// <summary>
///     Internal helper class to apply an <see cref="ISpecification{T}" /> to an <see cref="IQueryable{T}" />.
///     This centralizes the logic for translating specification details (criteria, includes, ordering, pagination)
///     into an EF Core compatible query.
/// </summary>
/// <typeparam name="T">The type of the entity being queried.</typeparam>
internal static class SpecificationEvaluator<T> where T : class // Assuming T is a reference type for EF Core
{
    /// <summary>
    ///     Applies the given specification to the input queryable source.
    /// </summary>
    /// <param name="inputQuery">The original <see cref="IQueryable{T}" /> source.</param>
    /// <param name="specification">The <see cref="ISpecification{T}" /> to apply.</param>
    /// <returns>An <see cref="IQueryable{T}" /> with the specification applied.</returns>
    public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> specification)
    {
        var query = inputQuery;

        // Apply criteria
        if (specification.Criteria != null) query = query.Where(specification.Criteria);

        // Apply includes (expression-based)
        query = specification.Includes.Aggregate(query,
            (current, include) => current.Include(include));

        // Apply includes (string-based)
        query = specification.IncludeStrings.Aggregate(query,
            (current, include) => current.Include(include));

        // Apply ordering
        if (specification.OrderBy != null)
            query = query.OrderBy(specification.OrderBy);
        else if (specification.OrderByDescending != null)
            query = query.OrderByDescending(specification.OrderByDescending);

        // Apply pagination
        if (specification.IsPagingEnabled) query = query.Skip(specification.Skip).Take(specification.Take);

        return query;
    }
}