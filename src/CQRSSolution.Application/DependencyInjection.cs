using CQRSSolution.Application.Commands.CreateOrder;
using CQRSSolution.Application.Factories;
using CQRSSolution.Application.Validators;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CQRSSolution.Application;

/// <summary>
///     Static class for configuring dependency injection for application services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    ///     Adds application-layer services to the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
    public static void AddApplicationServices(this IServiceCollection services)
    {
        // Register MediatR - scans the current assembly for handlers and requests
        services.AddMediatR(typeof(CreateOrderCommand).Assembly);

        // Register FluentValidation validators - scans the current assembly for validators
        services.AddValidatorsFromAssemblyContaining<CreateOrderCommandValidator>();

        // Register Factories
        services.AddScoped<IOrderFactory, OrderFactory>();
        services.AddScoped<IOutboxMessageFactory, OutboxMessageFactory>();
        // If OutboxMessageFactory needs specific JsonSerializerOptions, 
        // you could register JsonSerializerOptions or use IOptions<JsonSerializerOptions>
        // For now; it uses its default or a constructor that takes options.

        // Register other application services like mappers, etc.
    }
}