using BarbeariaSaaS.Domain.Entities;

namespace BarbeariaSaaS.Application.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetUsersByTenantAsync(Guid tenantId);
    Task<bool> IsEmailAvailableAsync(string email);
} 