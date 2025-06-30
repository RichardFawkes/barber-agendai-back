using Microsoft.EntityFrameworkCore;
using BarbeariaSaaS.Application.Interfaces;
using BarbeariaSaaS.Domain.Entities;
using BarbeariaSaaS.Infrastructure.Data;

namespace BarbeariaSaaS.Infrastructure.Repositories;

public class TenantRepository : Repository<Tenant>, ITenantRepository
{
    private readonly ApplicationDbContext _context;

    public TenantRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Tenant?> GetBySubdomainAsync(string subdomain)
    {
        return await _dbSet
            .FirstOrDefaultAsync(t => t.Subdomain.ToLower() == subdomain.ToLower());
    }

    public async Task<bool> IsSubdomainAvailableAsync(string subdomain)
    {
        return !await _context.Tenants
            .AnyAsync(t => t.Subdomain.ToLower() == subdomain.ToLower());
    }

    public async Task<IEnumerable<Tenant>> GetActiveTenants()
    {
        return await _context.Tenants
            .Where(t => t.Status == TenantStatus.Active)
            .ToListAsync();
    }

    public async Task UpdateBusinessHoursAsync(Guid tenantId, List<BusinessHour> newBusinessHours)
    {
        // Remove existing business hours for this tenant
        var existingBusinessHours = await _context.BusinessHours
            .Where(bh => bh.TenantId == tenantId)
            .ToListAsync();

        _context.BusinessHours.RemoveRange(existingBusinessHours);

        // Add new business hours
        if (newBusinessHours.Any())
        {
            await _context.BusinessHours.AddRangeAsync(newBusinessHours);
        }

        await _context.SaveChangesAsync();
    }
} 