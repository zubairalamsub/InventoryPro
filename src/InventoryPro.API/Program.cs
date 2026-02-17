using Carter;
using Hangfire;
using InventoryPro.API.Middleware;
using InventoryPro.Application;
using InventoryPro.Infrastructure;
using InventoryPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Scalar.AspNetCore;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting InventoryPro API");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Configure Serilog
    builder.Host.UseSerilog((context, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    // Add services
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // Add Carter for minimal API endpoints
    builder.Services.AddCarter();

    // Add OpenAPI
    builder.Services.AddOpenApi();

    // Add SignalR
    builder.Services.AddSignalR();

    // Add CORS
    var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string>()?.Split(',', StringSplitOptions.RemoveEmptyEntries)
        ?? new[] { "http://localhost:4200", "https://localhost:4200" };

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngularApp", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    // Add Health Checks
    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "database");

    // Add ProblemDetails and Exception Handler
    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    var app = builder.Build();

    // Configure middleware pipeline
    app.UseExceptionHandler();
    app.UseStatusCodePages();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.Title = "InventoryPro API";
            options.Theme = ScalarTheme.BluePlanet;
        });
    }

    app.UseSerilogRequestLogging();

    app.UseCors("AllowAngularApp");

    app.UseAuthentication();
    app.UseAuthorization();

    // Custom middleware
    app.UseMiddleware<TenantMiddleware>();

    // Map Health endpoints
    app.MapHealthChecks("/health");

    // Map Carter endpoints
    app.MapCarter();

    // Map Hangfire dashboard (admin only)
    app.MapHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireAuthorizationFilter() }
    });

    // Apply migrations (can be enabled in production via env var)
    var runMigrations = app.Environment.IsDevelopment() ||
        Environment.GetEnvironmentVariable("RUN_MIGRATIONS")?.ToLower() == "true";

    if (runMigrations)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        // Seed data only in development
        if (app.Environment.IsDevelopment())
        {
            var seeder = scope.ServiceProvider.GetService<DatabaseSeeder>();
            if (seeder != null)
            {
                await seeder.SeedAsync();
            }
        }
    }

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for integration tests
public partial class Program { }
