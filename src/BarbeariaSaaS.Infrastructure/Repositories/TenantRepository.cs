using Microsoft.EntityFrameworkCore;
using BarbeariaSaaS.Application.Interfaces;
using BarbeariaSaaS.Domain.Entities;
using BarbeariaSaaS.Infrastructure.Data;

namespace BarbeariaSaaS.Infrastructure.Repositories;

public class TenantRepository : Repository<Tenant>, ITenantRepository
{
    public TenantRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Tenant?> GetBySubdomainAsync(string subdomain)
    {
        return await _dbSet
            .FirstOrDefaultAsync(t => t.Subdomain.ToLower() == subdomain.ToLower());
    }

    public async Task<bool> IsSubdomainAvailableAsync(string subdomain)
    {
        return !await _dbSet
            .AnyAsync(t => t.Subdomain.ToLower() == subdomain.ToLower());
    }

    public async Task<IEnumerable<Tenant>> GetActiveTenants()
    {
        return await _dbSet
            .Where(t => t.Status == TenantStatus.Active)
            .ToListAsync();
    }
} 