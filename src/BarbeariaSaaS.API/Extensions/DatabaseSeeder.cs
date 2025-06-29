using System.Security.Cryptography;
using System.Text;
using BarbeariaSaaS.Domain.Entities;
using BarbeariaSaaS.Infrastructure.Data;
using BarbeariaSaaS.Infrastructure.Services;

namespace BarbeariaSaaS.API.Extensions;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (context.Tenants.Any())
        {
            return; // Database already seeded
        }

        // Create test tenant
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Barbearia do João",
            Subdomain = "joao",
            Description = "A melhor barbearia da cidade",
            Phone = "(11) 99999-9999",
            Email = "contato@barbearianojoao.com",
            Address = "Rua das Flores, 123 - São Paulo, SP",
            Status = TenantStatus.Active,
            Plan = SubscriptionPlan.Professional,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Branding = new TenantBranding
            {
                Colors = new BrandingColors
                {
                    Primary = "#2563EB",
                    Accent = "#10B981",
                    Secondary = "#6B7280",
                    Background = "#FFFFFF",
                    Text = "#1F2937",
                    Muted = "#9CA3AF"
                },
                Logo = new BrandingLogo
                {
                    Url = "/images/logo-joao.png",
                    Alt = "Barbearia do João"
                },
                Fonts = new BrandingFonts
                {
                    Primary = "Roboto",
                    Secondary = "Roboto"
                }
            },
            Settings = new TenantSettings
            {
                BusinessHours = new List<BusinessHourSettings>
                {
                    new() { Id = "1", DayOfWeek = 1, IsOpen = true, OpenTime = "08:00", CloseTime = "18:00" },
                    new() { Id = "2", DayOfWeek = 2, IsOpen = true, OpenTime = "08:00", CloseTime = "18:00" },
                    new() { Id = "3", DayOfWeek = 3, IsOpen = true, OpenTime = "08:00", CloseTime = "18:00" },
                    new() { Id = "4", DayOfWeek = 4, IsOpen = true, OpenTime = "08:00", CloseTime = "18:00" },
                    new() { Id = "5", DayOfWeek = 5, IsOpen = true, OpenTime = "08:00", CloseTime = "18:00" },
                    new() { Id = "6", DayOfWeek = 6, IsOpen = true, OpenTime = "08:00", CloseTime = "16:00" },
                    new() { Id = "7", DayOfWeek = 0, IsOpen = false, OpenTime = "00:00", CloseTime = "00:00" }
                },
                Booking = new BookingSettings
                {
                    AllowOnlineBooking = true,
                    MaxAdvanceDays = 30,
                    MinAdvanceHours = 2,
                    AllowCancellation = true,
                    CancellationDeadlineHours = 24
                }
            }
        };

        context.Tenants.Add(tenant);

        // Create hash for password "admin123"
        var hashedPassword = HashPassword("admin123");

        // Create admin user
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Name = "João Silva",
            Email = "admin@barbearianojoao.com",
            PasswordHash = hashedPassword,
            Phone = "(11) 98888-8888",
            Role = UserRole.TenantAdmin,
            TenantId = tenant.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(adminUser);

        // Create service category
        var category = new ServiceCategory
        {
            TenantId = tenant.Id,
            Name = "Cortes",
            Description = "Cortes de cabelo masculino",
            Color = "#2563EB",
            IsActive = true
        };

        context.ServiceCategories.Add(category);
        await context.SaveChangesAsync(); // Save to get category ID

        // Create services
        var services = new List<Service>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Name = "Corte Masculino",
                Description = "Corte de cabelo masculino tradicional",
                Price = 35.00m,
                DurationMinutes = 30,
                Color = "#2563EB",
                CategoryId = category.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Name = "Barba",
                Description = "Aparar e modelar barba",
                Price = 25.00m,
                DurationMinutes = 20,
                Color = "#10B981",
                CategoryId = category.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Name = "Corte + Barba",
                Description = "Pacote completo: corte de cabelo e barba",
                Price = 50.00m,
                DurationMinutes = 45,
                Color = "#8B5CF6",
                CategoryId = category.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Services.AddRange(services);

        // Create test customers
        var customers = new List<Customer>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Name = "Carlos Pereira",
                Email = "carlos@email.com",
                Phone = "(11) 97777-7777",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Name = "Roberto Santos",
                Email = "roberto@email.com",
                Phone = "(11) 96666-6666",
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Customers.AddRange(customers);

        // Create test bookings
        var tomorrow = DateTime.Today.AddDays(1);
        var bookings = new List<Booking>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                ServiceId = services[0].Id,
                CustomerId = customers[0].Id,
                CustomerName = customers[0].Name,
                CustomerEmail = customers[0].Email,
                CustomerPhone = customers[0].Phone,
                BookingDate = tomorrow,
                BookingTime = new TimeSpan(9, 0, 0),
                ServicePrice = services[0].Price,
                Status = BookingStatus.Confirmed,
                Notes = "Cliente preferencial",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                ServiceId = services[2].Id,
                CustomerId = customers[1].Id,
                CustomerName = customers[1].Name,
                CustomerEmail = customers[1].Email,
                CustomerPhone = customers[1].Phone,
                BookingDate = tomorrow,
                BookingTime = new TimeSpan(14, 30, 0),
                ServicePrice = services[2].Price,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        context.Bookings.AddRange(bookings);

        await context.SaveChangesAsync();
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var salt = GenerateSalt();
        var saltedPassword = password + salt;
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
        var hash = Convert.ToBase64String(hashedBytes);
        return $"{salt}:{hash}";
    }

    private static string GenerateSalt()
    {
        var saltBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }
} 