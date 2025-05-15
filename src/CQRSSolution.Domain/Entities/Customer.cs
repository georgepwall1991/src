namespace CQRSSolution.Domain.Entities;

/// <summary>
///     Represents a customer in the system.
/// </summary>
public class Customer
{
    // Private constructor for EF Core and factory method
    private Customer()
    {
    }

    /// <summary>
    ///     Gets or sets the unique identifier for the customer.
    /// </summary>
    public Guid CustomerId { get; private set; }

    /// <summary>
    ///     Gets or sets the name of the customer.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the email address of the customer. This is often used as a unique identifier.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the date and time when the customer was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    ///     Navigation property for the orders placed by this customer.
    /// </summary>
    public virtual ICollection<Order> Orders { get; private set; } = new List<Order>();

    /// <summary>
    ///     Creates a new <see cref="Customer" /> instance.
    /// </summary>
    /// <param name="name">The name of the customer.</param>
    /// <param name="email">The email address of the customer.</param>
    /// <returns>A new customer instance.</returns>
    /// <exception cref="ArgumentException">Thrown if name or email is null or whitespace.</exception>
    public static Customer Create(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Customer name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Customer email cannot be empty.", nameof(email));

        return new Customer
        {
            CustomerId = Guid.NewGuid(),
            Name = name,
            Email = email,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}