using System.Linq.Expressions;
using CQRSSolution.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CQRSSolution.Infrastructure.Persistence.Repositories;

/// <summary>
///     Provides a generic repository implementation for entities of type <typeparamref name="TEntity" /> using Entity
///     Framework Core.
/// </summary>
/// <typeparam name="TEntity">The type of the entity. Must be a class.</typeparam>
public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    /// <summary>
    ///     The database context instance.
    /// </summary>
    protected readonly ApplicationDbContext _dbContext;

    /// <summary>
    ///     The DbSet for the entity type <typeparamref name="TEntity" />.
    /// </summary>
    protected readonly DbSet<TEntity> _dbSet;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Repository{TEntity}" /> class.
    /// </summary>
    /// <param name="dbContext">The application database context.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dbContext" /> is null.</exception>
    public Repository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dbSet = _dbContext.Set<TEntity>();
    }

    /// <inheritdoc />
    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public virtual void Update(TEntity entity)
    {
        _dbSet.Update(entity);
    }

    /// <inheritdoc />
    public virtual void Remove(TEntity entity)
    {
        _dbSet.Remove(entity);
    }

    /// <inheritdoc />
    public virtual void RemoveRange(IEnumerable<TEntity> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> GetFirstOrDefaultAsync(ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<List<TEntity>> ListAsync(ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<int> CountAsync(ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification, true).CountAsync(cancellationToken);
    }

    /// <summary>
    ///     Applies the given specification to the <see cref="_dbSet" />.
    /// </summary>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="isCountOperation">
    ///     Indicates if the operation is for a count, which might omit ordering and pagination for
    ///     efficiency.
    /// </param>
    /// <returns>An <see cref="IQueryable{TEntity}" /> with the specification applied.</returns>
    private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification, bool isCountOperation = false)
    {
        // For count operations, we can potentially optimize by not applying OrderBy or Pagination if the evaluator supports it.
        // The current SpecificationEvaluator applies them regardless, which is fine for most cases.
        // If isCountOperation is true and we want to optimize, the SpecificationEvaluator would need to be aware of it.
        return SpecificationEvaluator<TEntity>.GetQuery(_dbSet.AsQueryable(), specification);
    }
}