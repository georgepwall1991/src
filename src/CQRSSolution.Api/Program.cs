using CQRSSolution.Api;
using CQRSSolution.Application;
using CQRSSolution.Application.Commands.CreateOrder;
using CQRSSolution.Application.Interfaces;
using CQRSSolution.Infrastructure;
using CQRSSolution.Infrastructure.AzureServiceBus;
using CQRSSolution.Infrastructure.BackgroundServices;
using CQRSSolution.Infrastructure.Persistence;
using CQRSSolution.Infrastructure.Persistence.Repositories;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. Configure DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));

// 2. Register Application Layer services
// MediatR - Scans the assembly where CreateOrderCommand is located (CQRSSolution.Application)
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly));

// 3. Register Infrastructure Layer services
// Repositories
builder.Services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();

// Event Bus Publisher (Placeholder)
builder.Services.AddSingleton<IEventBusPublisher, EventBusPublisher>(); // Singleton if stateless, Scoped if it has scoped dependencies

// Background Service for Outbox Processing
builder.Services.AddHostedService<OutboxProcessorService>();

// API Controllers
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "v1",
        Title = "CQRSSolution API",
        Description = "API for managing orders using CQRS and Outbox Pattern."
    });

    // Optionally, include XML comments for Swagger UI if set up in .csproj
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CQRSSolution API V1");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at app root
    });
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();