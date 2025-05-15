using CQRSSolution.Application.Interfaces;
using CQRSSolution.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CQRSSolution.Infrastructure.Persistence.Repositories;

/// <summary>
///     Provides an implementation of <see cref="ICustomerRepository" /> using Entity Framework Core.
/// </summary>
public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CustomerRepository" /> class.
    /// </summary>
    /// <param name="dbContext">The application database context.</param>
    public CustomerRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    /// <inheritdoc />
    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            // Or throw ArgumentNullException, depending on desired contract
            return null;
        return await _dbSet.FirstOrDefaultAsync(c => c.Email == email, cancellationToken);
    }
}