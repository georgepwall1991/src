using CQRSSolution.Api;
using CQRSSolution.Application;
using CQRSSolution.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApiServices();

var app = builder.Build();

app.ConfigureApiMiddleware();

app.Run();