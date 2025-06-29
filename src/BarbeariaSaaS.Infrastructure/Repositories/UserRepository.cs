using Microsoft.EntityFrameworkCore;
using BarbeariaSaaS.Application.Interfaces;
using BarbeariaSaaS.Domain.Entities;
using BarbeariaSaaS.Infrastructure.Data;

namespace BarbeariaSaaS.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<IEnumerable<User>> GetUsersByTenantAsync(Guid tenantId)
    {
        return await _dbSet
            .Where(u => u.TenantId == tenantId)
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    public async Task<bool> IsEmailAvailableAsync(string email)
    {
        return !await _dbSet
            .AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }
} 