---

## 7. Dependency Injection and Persistence Strategy

This section outlines how Dependency Injection (DI) and data persistence are managed within the CQRSSolution.

### 7.1. Dependency Injection (DI)

The application leverages the built-in .NET Core DI container. Service registrations are organized by layer to maintain separation of concerns and improve modularity.

- **API Layer (`ProjectName.Api/Program.cs`):**
- Acts as the composition root.
- Initializes and configures the web application builder.
- Calls extension methods from the `Application` and `Infrastructure` layers to register their respective services.
- Registers API-specific services like controllers, Swagger/OpenAPI documentation, health checks, and global JSON serialization options.
- Configures FluentValidation for automatic request model validation at the API boundary.

- **Application Layer (`ProjectName.Application/DependencyInjection.cs`):
- Provides an `AddApplicationServices(this IServiceCollection services)` extension method.
- Registers MediatR and scans its assembly for command/query handlers and requests.
- Registers FluentValidation validators from its assembly.
- Registers application-specific services like factories (e.g., `IOrderFactory`, `IOutboxMessageFactory`).
- Service Lifetimes: Application services are typically registered as `Scoped` or `Transient` depending on their nature. MediatR handlers are often transient, resolved per request.

- **Infrastructure Layer (`ProjectName.Infrastructure/DependencyInjection.cs`):
- Provides an `AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)` extension method.
- Registers the `ApplicationDbContext` with the SQL Server provider, sourcing the connection string from `IConfiguration` and configuring the migrations assembly.
- Registers `IApplicationDbContext` to resolve to `ApplicationDbContext`, allowing application services to depend on the interface for easier testing and abstraction.
- Registers repository implementations (e.g., `IRepository<>`, `IOrderRepository`, `IOutboxMessageRepository`) typically as `Scoped`.
- Registers the `IUnitOfWork` implementation as `Scoped`.
- Registers other infrastructure services such as `IEventBusPublisher` (e.g., `AzureServiceBusPublisher` as `Singleton` if thread-safe and stateless, or `Scoped` if it has scoped dependencies or state per request/scope) and `DomainEventDeserializer`.
- Registers `IHostedService` implementations like `OutboxProcessorService` for background tasks.

**Benefits of this DI Strategy:**
- **Modularity:** Each layer manages its own service registrations, making the system easier to understand and maintain.
- **Testability:** Clear separation of concerns and dependency on abstractions (interfaces) facilitate unit testing with mocks.
- **Maintainability:** Changes within one layer's DI setup are less likely to impact other layers.
- **Clean Composition Root:** `Program.cs` remains clean, focusing on high-level application composition.

### 7.2. Persistence

Data persistence is primarily handled using Entity Framework Core (EF Core) and the Repository pattern.

- **`ApplicationDbContext` (`ProjectName.Infrastructure/Persistence`):
- The primary EF Core `DbContext` for the application.
- Implements `IApplicationDbContext` (defined in `ProjectName.Application`) to provide an abstraction for the application layer to work with.
- Contains `DbSet<>` properties for all persisted entities (e.g., `Orders`, `OrderItems`, `Customers`, `OutboxMessages`).
- Includes entity configurations (relationships, constraints, keys) within its `OnModelCreating` method.

- **Repository Pattern (`ProjectName.Infrastructure/Persistence/Repositories`):
- Generic `IRepository<>` and `Repository<>` provide common data access operations (Add, Get, Update, Delete).
- Specific repositories (e.g., `IOrderRepository`, `ICustomerRepository`) can be implemented if entity-specific query methods are needed beyond the generic set.
- Repositories are injected with `ApplicationDbContext` and encapsulate EF Core specific data access logic.
- They abstract the data source from the application layer, promoting cleaner architecture and easier testing of application logic.

- **Unit of Work (`ProjectName.Infrastructure/Persistence/UnitOfWork.cs` & `ProjectName.Application/Interfaces/IUnitOfWork.cs`):
- The `IUnitOfWork` interface (defined in Application) typically exposes a `SaveChangesAsync()` method.
- The `UnitOfWork` implementation (in Infrastructure) often wraps the `ApplicationDbContext`'s `SaveChangesAsync()`.
- This pattern ensures that all changes within a single business operation (typically a command handler) are saved atomically as part of a single transaction managed by EF Core.
- While EF Core's `DbContext` itself acts as a Unit of Work, explicitly defining `IUnitOfWork` can provide a clearer abstraction for transaction management, especially if multiple repositories are involved in a single operation.

- **Transactions:**
- EF Core manages transactions automatically for `SaveChanges()` calls by default.
- For more complex scenarios, like the Transactional Outbox pattern, explicit transaction control (`_dbContext.Database.BeginTransactionAsync()`) is used within command handlers to ensure that business data changes and outbox message creation are part of the same atomic database transaction.

**Benefits of this Persistence Strategy:**
- **Abstraction:** The Application layer is decoupled from EF Core specifics by depending on interfaces like `IApplicationDbContext`, `IRepository<>`, and `IUnitOfWork`.
- **Testability:** Application logic can be tested by mocking these persistence interfaces.
- **Data Integrity:** Leveraging EF Core transactions (implicit or explicit) helps maintain data consistency, crucial for patterns like the Transactional Outbox.
- **Centralized Data Logic:** Repositories provide a clear place for data access logic, and `ApplicationDbContext` centralizes entity configurations.
