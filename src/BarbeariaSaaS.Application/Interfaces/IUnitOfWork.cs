namespace BarbeariaSaaS.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ITenantRepository Tenants { get; }
    IUserRepository Users { get; }
    IServiceRepository Services { get; }
    IBookingRepository Bookings { get; }
    ICustomerRepository Customers { get; }
    IRepository<BarbeariaSaaS.Domain.Entities.ServiceCategory> ServiceCategories { get; }
    IRepository<BarbeariaSaaS.Domain.Entities.BusinessHour> BusinessHours { get; }
    IRepository<BarbeariaSaaS.Domain.Entities.File> Files { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
} 