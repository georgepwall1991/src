using CQRSSolution.Domain.Entities;

namespace CQRSSolution.Application.Interfaces;

/// <summary>
///     Repository interface for Customer specific operations.
/// </summary>
public interface ICustomerRepository : IRepository<Customer>
{
    /// <summary>
    ///     Gets a customer by their email address.
    /// </summary>
    /// <param name="email">The email address of the customer.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>The customer if found; otherwise, null.</returns>
    Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}