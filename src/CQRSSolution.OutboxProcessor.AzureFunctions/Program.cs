using CQRSSolution.Application.Interfaces;
using CQRSSolution.Infrastructure.AzureServiceBus;
using CQRSSolution.Infrastructure.Persistence;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((hostContext, services) =>
    {
        // Application Insights telemetry
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Configure JSON serialization options
        services.AddSingleton(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Database configuration
        ConfigureDatabase(hostContext, services);

        // Service Bus configuration
        services.Configure<ServiceBusPublisherOptions>(hostContext.Configuration.GetSection("ServiceBus"));
        services.AddSingleton<IEventBusPublisher, ServiceBusPublisher>();
        
        // Logging configuration
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddFilter("CQRSSolution", LogLevel.Debug);
        });
    })
    .Build();

await host.RunAsync();
return;

void ConfigureDatabase(HostBuilderContext context, IServiceCollection services)
{
    var connectionString = context.Configuration["SqlDatabaseConnectionString"];
    
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("SQL connection string is not configured.");
    }
    
    services.AddDbContext<ApplicationDbContext>(options => 
        options.UseSqlServer(connectionString));
}