using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using BarbeariaSaaS.Application.Interfaces;
using BarbeariaSaaS.Domain.Entities;
using BarbeariaSaaS.Infrastructure.Data;

namespace BarbeariaSaaS.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    private ITenantRepository? _tenants;
    private IUserRepository? _users;
    private IServiceRepository? _services;
    private IBookingRepository? _bookings;
    private ICustomerRepository? _customers;
    private IRepository<ServiceCategory>? _serviceCategories;
    private IRepository<BusinessHour>? _businessHours;
    private IRepository<BarbeariaSaaS.Domain.Entities.File>? _files;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public ITenantRepository Tenants =>
        _tenants ??= new TenantRepository(_context);

    public IUserRepository Users =>
        _users ??= new UserRepository(_context);

    public IServiceRepository Services =>
        _services ??= new ServiceRepository(_context);

    public IBookingRepository Bookings =>
        _bookings ??= new BookingRepository(_context);

    public ICustomerRepository Customers =>
        _customers ??= new CustomerRepository(_context);

    public IRepository<ServiceCategory> ServiceCategories =>
        _serviceCategories ??= new Repository<ServiceCategory>(_context);

    public IRepository<BusinessHour> BusinessHours =>
        _businessHours ??= new Repository<BusinessHour>(_context);

    public IRepository<BarbeariaSaaS.Domain.Entities.File> Files =>
        _files ??= new Repository<BarbeariaSaaS.Domain.Entities.File>(_context);

    public async Task<int> SaveChangesAsync()
    {
        // Update timestamps
        var entries = _context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Tenant tenant)
            {
                if (entry.State == EntityState.Added)
                    tenant.CreatedAt = DateTime.UtcNow;
                tenant.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is Booking booking)
            {
                if (entry.State == EntityState.Modified)
                    booking.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
} 