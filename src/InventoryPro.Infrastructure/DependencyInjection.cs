using System.Text;
using Hangfire;
using Hangfire.PostgreSql;
using InventoryPro.Application.Common.Interfaces;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Interfaces;
using InventoryPro.Infrastructure.Identity;
using InventoryPro.Infrastructure.Persistence;
using InventoryPro.Infrastructure.Persistence.Interceptors;
using InventoryPro.Infrastructure.Persistence.Repositories;
using InventoryPro.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace InventoryPro.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add interceptors
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();
        services.AddScoped<TenantInterceptor>();
        services.AddScoped<DomainEventInterceptor>();

        // Add DbContext
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            });

            // Add interceptors
            var auditInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();
            var softDeleteInterceptor = sp.GetRequiredService<SoftDeleteInterceptor>();
            var tenantInterceptor = sp.GetRequiredService<TenantInterceptor>();
            var domainEventInterceptor = sp.GetRequiredService<DomainEventInterceptor>();

            options.AddInterceptors(
                tenantInterceptor,
                auditInterceptor,
                softDeleteInterceptor,
                domainEventInterceptor);
        });

        // Add Identity
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Add JWT Authentication
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT settings are not configured");

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            // Configure SignalR authentication
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;

                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        });

        // Add Authorization
        services.AddAuthorizationBuilder()
            .AddPolicy("RequireTenant", policy =>
                policy.RequireClaim("tenant_id"))
            .AddPolicy("RequireAdmin", policy =>
                policy.RequireRole("Admin", "SystemAdmin"))
            .AddPolicy("RequireSystemAdmin", policy =>
                policy.RequireRole("SystemAdmin"));

        // Add Hangfire (optional - disable in production if not needed)
        var enableHangfire = configuration.GetValue<bool>("EnableHangfire", false);
        if (enableHangfire)
        {
            var hangfireConnectionString = configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(hangfireConnectionString))
            {
                services.AddHangfire(config =>
                {
                    config.UsePostgreSqlStorage(options =>
                    {
                        options.UseNpgsqlConnection(hangfireConnectionString);
                    });
                });
                services.AddHangfireServer();
            }
        }

        // Register services
        services.AddScoped<IDateTime, DateTimeService>();
        services.AddScoped<ITenantProvider, TenantProvider>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<DatabaseSeeder>();

        // Add HttpContextAccessor
        services.AddHttpContextAccessor();

        // Add HybridCache (Redis + Memory)
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "InventoryPro:";
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

#pragma warning disable EXTEXP0018
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromMinutes(1)
            };
        });
#pragma warning restore EXTEXP0018

        return services;
    }
}
