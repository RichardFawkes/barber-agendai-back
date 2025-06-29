using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MediatR;
using BarbeariaSaaS.Application.Interfaces;
using BarbeariaSaaS.Application.Mappings;
using BarbeariaSaaS.Infrastructure.Data;
using BarbeariaSaaS.Infrastructure.Repositories;
using BarbeariaSaaS.Infrastructure.Services;
using BarbeariaSaaS.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Database Configuration - Simplified and robust
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
var isProduction = builder.Environment.IsProduction();

// Log database configuration for debugging
Console.WriteLine($"=== DATABASE CONFIGURATION ===");
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"IsProduction: {isProduction}");
Console.WriteLine($"DATABASE_URL present: {!string.IsNullOrEmpty(databaseUrl)}");
if (!string.IsNullOrEmpty(databaseUrl))
{
    var uri = new Uri(databaseUrl);
    Console.WriteLine($"DATABASE_URL host: {uri.Host}");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (!string.IsNullOrEmpty(databaseUrl))
    {
        // Production - PostgreSQL from DATABASE_URL (Render.com)
        Console.WriteLine("✅ USING: PostgreSQL from DATABASE_URL");
        
        try
        {
            var uri = new Uri(databaseUrl);
            var username = uri.UserInfo.Split(':')[0];
            var password = uri.UserInfo.Split(':')[1];
            var postgresConnectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.LocalPath.Substring(1)};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
            
            Console.WriteLine($"PostgreSQL Host: {uri.Host}:{uri.Port}");
            Console.WriteLine($"PostgreSQL Database: {uri.LocalPath.Substring(1)}");
            options.UseNpgsql(postgresConnectionString);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR parsing DATABASE_URL: {ex.Message}");
            Console.WriteLine("Falling back to SQLite...");
            var fallbackConnection = "Data Source=barbearia_fallback.db";
            options.UseSqlite(fallbackConnection);
        }
    }
    else
    {
        // Always use SQLite when DATABASE_URL is not available
        // This ensures we NEVER try to use SQL Server LocalDB
        var sqliteFile = isProduction ? "barbearia_production.db" : "barbearia_development.db";
        var sqliteConnection = $"Data Source={sqliteFile}";
        
        Console.WriteLine($"✅ USING: SQLite - {sqliteFile}");
        Console.WriteLine($"SQLite connection: {sqliteConnection}");
        options.UseSqlite(sqliteConnection);
    }
});

// Repository Pattern
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

// Authentication Services
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// MediatR
builder.Services.AddMediatR(typeof(BarbeariaSaaS.Application.Features.Auth.Commands.LoginCommand).Assembly);

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? throw new InvalidOperationException("JWT Secret Key not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "BarbeariaSaaS",
        ValidAudience = jwtSettings["Audience"] ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "BarbeariaSaaS-Users",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// Authorization
builder.Services.AddAuthorization();

// CORS - Allow frontend domains
var allowedOrigins = new List<string> { "http://localhost:3000", "https://localhost:3001" };

// Add production frontend URL if available
var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL");
if (!string.IsNullOrEmpty(frontendUrl))
{
    allowedOrigins.Add(frontendUrl);
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins.ToArray())
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "BarbeariaSaaS API", 
        Version = "v1",
        Description = "API para Sistema de Agendamento de Barbearias SaaS"
    });

    // JWT Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Migrate and Seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("=== STARTING DATABASE MIGRATION ===");
        logger.LogInformation("Database Provider: {Provider}", context.Database.ProviderName);
        
        // Ensure database is created and migrations are applied
        logger.LogInformation("Applying migrations...");
        await context.Database.MigrateAsync();
        logger.LogInformation("✅ Database migration completed successfully");
        
        // Seed data
        logger.LogInformation("Starting database seeding...");
        await DatabaseSeeder.SeedAsync(context);
        logger.LogInformation("✅ Database seeding completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ An error occurred while migrating or seeding the database");
        
        // In production, we want to fail fast if database is not accessible
        if (app.Environment.IsProduction())
        {
            logger.LogCritical("Database migration failed in production. Stopping application.");
            throw;
        }
        
        // In development, log and continue
        logger.LogWarning("Continuing without database migration/seeding due to error");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BarbeariaSaaS API v1");
        c.RoutePrefix = string.Empty; // Makes Swagger available at root URL
    });
}

// Always enable Swagger in production for API documentation
if (app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BarbeariaSaaS API v1");
        c.RoutePrefix = "docs"; // Makes Swagger available at /docs
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health Check endpoint
app.MapGet("/health", (IServiceProvider serviceProvider) => 
{
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    var environment = serviceProvider.GetRequiredService<IWebHostEnvironment>();
    
    var dbType = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DATABASE_URL")) ? "PostgreSQL" : "SQLite";
    
    logger.LogInformation("Health check requested - Status: Healthy, Database: {DbType}", dbType);
    
    return Results.Ok(new { 
        Status = "Healthy", 
        Timestamp = DateTime.UtcNow,
        Environment = environment.EnvironmentName,
        Database = dbType,
        HasDatabaseUrl = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DATABASE_URL"))
    });
});

Console.WriteLine("=== APPLICATION STARTING ===");
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"URLs: {Environment.GetEnvironmentVariable("ASPNETCORE_URLS")}");

app.Run();
