using BarbeariaSaaS.Domain.Entities;
using BarbeariaSaaS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BarbeariaSaaS.API.Scripts;

public static class SeedTestData
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Verifica se já existe o tenant tiopatinhas
        var existingTenant = await context.Tenants
            .FirstOrDefaultAsync(t => t.Subdomain == "tiopatinhas");

        if (existingTenant != null)
        {
            Console.WriteLine("Tenant 'tiopatinhas' já existe!");
            return;
        }

        // Criar tenant de teste
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Barbearia Tio Patinhas",
            Subdomain = "tiopatinhas",
            Email = "contato@tiopatinhas.com",
            Phone = "(11) 99999-9999",
            Address = "Rua dos Testes, 123",
            Status = TenantStatus.Active,
            Plan = SubscriptionPlan.Basic,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Tenants.Add(tenant);

        // Criar um serviço de teste
        var service = new Service
        {
            Id = Guid.Parse("02943c8d-402d-466d-b470-3d84abc83d5a"),
            TenantId = tenant.Id,
            Name = "Corte Masculino",
            Description = "Corte de cabelo masculino tradicional",
            DurationMinutes = 30,
            Price = 25.00m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Services.Add(service);

        // Criar configurações básicas do tenant
        var settings = new TenantSetting
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            SlotDurationMinutes = 30,
            AdvanceBookingDays = 30,
            MaxBookingsPerDay = 50,
            BookingBufferMinutes = 0,
            Timezone = "America/Sao_Paulo",
            AutoConfirmBookings = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.TenantSettings.Add(settings);

        await context.SaveChangesAsync();
        
        Console.WriteLine($"Tenant '{tenant.Name}' criado com sucesso!");
        Console.WriteLine($"Subdomain: {tenant.Subdomain}");
        Console.WriteLine($"Service ID: {service.Id}");
    }
} 