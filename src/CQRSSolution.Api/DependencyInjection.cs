using FluentValidation.AspNetCore;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;

namespace CQRSSolution.Api;

public static class DependencyInjection
{
    public static void AddApiServices(this IServiceCollection services)
    {
        services.AddControllers();

        // FluentValidation ASP.NET Core integration
        services.AddFluentValidationAutoValidation();
        // services.AddFluentValidationClientsideAdapters(); // If using client-side validation

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "CQRS Solution API",
                Description = "API for the CQRS Solution demonstration project."
            });
        });

        // Note: Health check endpoint mapping is done in Program.cs as it uses app.MapHealthChecks
        // However, the core health check services are added in Infrastructure & potentially other layers.
    }

    // Helper method to configure OpenAPI/Swagger middleware, if needed outside Program.cs main flow.
    // For this project, it's simple enough to keep in Program.cs
    public static void ConfigureApiMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "CQRS Solution API V1");
                options.RoutePrefix = string.Empty;
            });
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
    }
}