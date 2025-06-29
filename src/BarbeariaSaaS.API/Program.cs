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

// Database Configuration - Smart detection
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var isProduction = builder.Environment.IsProduction();

// Log database configuration for debugging
Console.WriteLine($"=== DATABASE CONFIGURATION ===");
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"IsProduction: {isProduction}");
Console.WriteLine($"DATABASE_URL present: {!string.IsNullOrEmpty(databaseUrl)}");
Console.WriteLine($"ConnectionString from config: {connectionString}");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (!string.IsNullOrEmpty(databaseUrl))
    {
        // Cloud Production - PostgreSQL from DATABASE_URL (Render.com, Heroku, etc.)
        Console.WriteLine("✅ USING: PostgreSQL from DATABASE_URL (Cloud)");
        
        try
        {
            var uri = new Uri(databaseUrl);
            var username = uri.UserInfo.Split(':')[0];
            var password = uri.UserInfo.Split(':')[1];
            
            // Use porta padrão do PostgreSQL (5432) se não especificada
            var port = uri.Port > 0 ? uri.Port : 5432;
            var database = uri.LocalPath.TrimStart('/');
            
            var postgresConnectionString = $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
            
            Console.WriteLine($"PostgreSQL Host: {uri.Host}:{port}");
            Console.WriteLine($"PostgreSQL Database: {database}");
            Console.WriteLine($"PostgreSQL Username: {username}");
            Console.WriteLine($"Connection String (without password): Host={uri.Host};Port={port};Database={database};Username={username};SSL Mode=Require;Trust Server Certificate=true");
            
            options.UseNpgsql(postgresConnectionString);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR parsing DATABASE_URL: {ex.Message}");
            Console.WriteLine($"DATABASE_URL value: {databaseUrl}");
            Console.WriteLine("Falling back to SQLite...");
            var fallbackConnection = "Data Source=barbearia_fallback.db";
            options.UseSqlite(fallbackConnection);
        }
    }
    else if (!string.IsNullOrEmpty(connectionString) && connectionString.Contains("localdb"))
    {
        // Local Development - SQL Server LocalDB (only if explicitly configured)
        Console.WriteLine("✅ USING: SQL Server LocalDB (Development)");
        Console.WriteLine($"Connection: {connectionString}");
        options.UseSqlServer(connectionString);
    }
    else if (!string.IsNullOrEmpty(connectionString) && connectionString.Contains("Data Source"))
    {
        // SQLite (from config or fallback)
        Console.WriteLine("✅ USING: SQLite (from configuration)");
        Console.WriteLine($"Connection: {connectionString}");
        options.UseSqlite(connectionString);
    }
    else
    {
        // Ultimate fallback - SQLite
        var sqliteFile = isProduction ? "barbearia_production.db" : "barbearia_development.db";
        var sqliteConnection = $"Data Source={sqliteFile}";
        
        Console.WriteLine($"✅ USING: SQLite (fallback) - {sqliteFile}");
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
        logger.LogInformation("=== STARTING DATABASE SETUP ===");
        logger.LogInformation("Database Provider: {Provider}", context.Database.ProviderName);
        
        var databaseProvider = context.Database.ProviderName;
        
        if (databaseProvider?.Contains("Sqlite") == true || databaseProvider?.Contains("Npgsql") == true)
        {
            // SQLite e PostgreSQL - Use EnsureCreated (mais simples e confiável)
            var dbType = databaseProvider.Contains("Sqlite") ? "SQLite" : "PostgreSQL";
            logger.LogInformation("Using EnsureCreated for {DbType} database...", dbType);
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("✅ {DbType} database created/verified successfully", dbType);
        }
        else
        {
            // SQL Server - Use Migrations
            logger.LogInformation("Applying migrations for {Provider}...", databaseProvider);
            await context.Database.MigrateAsync();
            logger.LogInformation("✅ Database migration completed successfully");
        }
        
        // Seed data
        logger.LogInformation("Starting database seeding...");
        await DatabaseSeeder.SeedAsync(context);
        logger.LogInformation("✅ Database seeding completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ An error occurred while setting up the database");
        
        // In production, we want to fail fast if database is not accessible
        if (app.Environment.IsProduction())
        {
            logger.LogCritical("Database setup failed in production. Stopping application.");
            throw;
        }
        
        // In development, log and continue
        logger.LogWarning("Continuing without database setup due to error");
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

// Health Check endpoint with detailed database info
app.MapGet("/health", (IServiceProvider serviceProvider) => 
{
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    var environment = serviceProvider.GetRequiredService<IWebHostEnvironment>();
    var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
    
    var dbProvider = context.Database.ProviderName ?? "Unknown";
    var hasDbUrl = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DATABASE_URL"));
    
    string dbType;
    if (hasDbUrl)
        dbType = "PostgreSQL (Cloud)";
    else if (dbProvider.Contains("SqlServer"))
        dbType = "SQL Server (Local)";
    else if (dbProvider.Contains("Sqlite"))
        dbType = "SQLite";
    else
        dbType = dbProvider;
    
    logger.LogInformation("Health check - Database: {DbType}, Provider: {Provider}", dbType, dbProvider);
    
    return Results.Ok(new { 
        Status = "Healthy", 
        Timestamp = DateTime.UtcNow,
        Environment = environment.EnvironmentName,
        Database = dbType,
        DatabaseProvider = dbProvider,
        HasDatabaseUrl = hasDbUrl
    });
});

Console.WriteLine("=== APPLICATION STARTING ===");
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"URLs: {Environment.GetEnvironmentVariable("ASPNETCORE_URLS")}");

app.Run();
