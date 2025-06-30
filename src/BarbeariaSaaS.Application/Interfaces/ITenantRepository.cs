using BarbeariaSaaS.Domain.Entities;

namespace BarbeariaSaaS.Application.Interfaces;

public interface ITenantRepository : IRepository<Tenant>
{
    Task<Tenant?> GetBySubdomainAsync(string subdomain);
    Task<bool> IsSubdomainAvailableAsync(string subdomain);
    Task<IEnumerable<Tenant>> GetActiveTenants();
    Task UpdateBusinessHoursAsync(Guid tenantId, List<BusinessHour> newBusinessHours);
} 