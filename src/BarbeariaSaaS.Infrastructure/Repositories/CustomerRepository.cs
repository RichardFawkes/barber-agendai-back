using Microsoft.EntityFrameworkCore;
using BarbeariaSaaS.Application.Interfaces;
using BarbeariaSaaS.Domain.Entities;
using BarbeariaSaaS.Infrastructure.Data;

namespace BarbeariaSaaS.Infrastructure.Repositories;

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Customer?> GetByEmailAsync(Guid tenantId, string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Email.ToLower() == email.ToLower());
    }

    public async Task<IEnumerable<Customer>> GetCustomersByTenantAsync(Guid tenantId)
    {
        return await _dbSet
            .Where(c => c.TenantId == tenantId)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
} 