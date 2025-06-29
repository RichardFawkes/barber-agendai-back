using Microsoft.EntityFrameworkCore;
using BarbeariaSaaS.Application.Interfaces;
using BarbeariaSaaS.Domain.Entities;
using BarbeariaSaaS.Infrastructure.Data;

namespace BarbeariaSaaS.Infrastructure.Repositories;

public class ServiceRepository : Repository<Service>, IServiceRepository
{
    public ServiceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Service>> GetActiveServicesByTenantAsync(Guid tenantId)
    {
        return await _dbSet
            .Include(s => s.Category)
            .Where(s => s.TenantId == tenantId && s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Service>> GetServicesByCategoryAsync(Guid tenantId, int categoryId)
    {
        return await _dbSet
            .Include(s => s.Category)
            .Where(s => s.TenantId == tenantId && s.CategoryId == categoryId)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }
} 