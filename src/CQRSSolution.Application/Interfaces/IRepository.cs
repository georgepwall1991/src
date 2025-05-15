using System.Linq.Expressions;

// Assuming a base class or marker interface for entities, adjust if not

namespace CQRSSolution.Application.Interfaces;

/// <summary>
///     Generic repository interface for basic CRUD operations and querying.
/// </summary>
/// <typeparam name="TEntity">The type of the entity. Must be a class.</typeparam>
public interface IRepository<TEntity> where TEntity : class // Consider adding a constraint like IEntity if you have one
{
    /// <summary>
    ///     Adds an entity to the underlying DbSet.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous add operation.</returns>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Adds a range of entities to the underlying DbSet.
    /// </summary>
    /// <param name="entities">The entities to add.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous add operation.</returns>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets an entity by its ID.
    /// </summary>
    /// <param name="id">The ID of the entity.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default); // Assuming Guid IDs

    /// <summary>
    ///     Gets all entities.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A list of all entities.</returns>
    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds entities based on a predicate.
    /// </summary>
    /// <param name="predicate">The predicate to filter entities.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A list of entities that match the predicate.</returns>
    Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the first entity that matches the predicate or null if no entity is found.
    /// </summary>
    /// <param name="predicate">The predicate to filter entities.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>The first entity matching the predicate, or null.</returns>
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates an entity. (Note: EF Core tracks changes, so this might just ensure it's attached and marked as modified if
    ///     needed)
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    void Update(TEntity entity); // Typically synchronous as it just marks state

    /// <summary>
    ///     Removes an entity.
    /// </summary>
    /// <param name="entity">The entity to remove.</param>
    void Remove(TEntity entity); // Typically synchronous

    /// <summary>
    ///     Removes a range of entities.
    /// </summary>
    /// <param name="entities">The entities to remove.</param>
    void RemoveRange(IEnumerable<TEntity> entities); // Typically synchronous

    /// <summary>
    ///     Gets the first entity that matches the specification or null if no entity is found.
    /// </summary>
    /// <param name="specification">The specification to filter and shape entities.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>The first entity matching the specification, or null.</returns>
    Task<TEntity?> GetFirstOrDefaultAsync(ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a list of all entities that match the specification.
    /// </summary>
    /// <param name="specification">The specification to filter and shape entities.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A list of entities that match the specification.</returns>
    Task<List<TEntity>> ListAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Counts the number of entities that match the specification.
    /// </summary>
    /// <param name="specification">The specification to filter entities.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>The total number of entities matching the specification.</returns>
    Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);
}