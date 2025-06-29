using BarbeariaSaaS.Domain.Entities;

namespace BarbeariaSaaS.Application.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(Guid tenantId, string email);
    Task<IEnumerable<Customer>> GetCustomersByTenantAsync(Guid tenantId);
} 