using BarbeariaSaaS.Domain.Entities;

namespace BarbeariaSaaS.Application.Interfaces;

public interface IServiceRepository : IRepository<Service>
{
    Task<IEnumerable<Service>> GetActiveServicesByTenantAsync(Guid tenantId);
    Task<IEnumerable<Service>> GetServicesByCategoryAsync(Guid tenantId, int categoryId);
} 