using System.Text.Json;
using CQRSSolution.Application.Interfaces;
using CQRSSolution.Infrastructure.BackgroundServices;
using CQRSSolution.Infrastructure.Persistence;
using CQRSSolution.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CQRSSolution.Infrastructure;

/// <summary>
///     Static class for configuring dependency injection for infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    ///     Adds infrastructure-layer services to the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="configuration">The application configuration, used for connection strings and other settings.</param>
    /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Configure JsonSerializerOptions - these are used by OutboxProcessorService for deserialization
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            PropertyNameCaseInsensitive = true // Important for robust deserialization
        };
        services.AddSingleton(jsonSerializerOptions);

        // Register IApplicationDbContext
        // The command handler needs IApplicationDbContext for SaveChangesAsync and Transaction management.
        // Repositories get ApplicationDbContext (concrete type) injected by DI.
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Register Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Event Bus Publisher
        // Assuming AzureServiceBusPublisher might take IConfiguration for its connection string
        // or that the connection string is configured globally and it resolves it.
        // If AzureServiceBusPublisher needs specific options like queue name from config, inject IConfiguration.
        services.AddSingleton<IEventBusPublisher, AzureServiceBusPublisher>();

        // Register DomainEventDeserializer
        // It requires JsonSerializerOptions. We assume Program.cs still registers JsonSerializerOptions as a singleton,
        // so it can be injected into DomainEventDeserializer's constructor if needed.
        // Or, if DomainEventDeserializer needs its own config, it can be done here.
        services.AddScoped<DomainEventDeserializer>();

        // Register Outbox Processor Background Service
        services.AddHostedService<OutboxProcessorService>();

        // Add Health Checks specific to Infrastructure
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>("database", HealthStatus.Degraded,
                new[] { "db", "infrastructure" })
            .AddAzureServiceBusQueue(
                configuration.GetConnectionString("ServiceBusConnection"),
                configuration["QueueName"] ?? "your-queue-name", // Read from config, fallback
                name: "azure_service_bus_queue",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "messaging", "infrastructure" });

        return services;
    }
}