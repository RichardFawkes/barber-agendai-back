using Microsoft.EntityFrameworkCore;
using BarbeariaSaaS.Domain.Entities;
using System.Text.Json;

namespace BarbeariaSaaS.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<ServiceCategory> ServiceCategories { get; set; }
    public DbSet<BusinessHour> BusinessHours { get; set; }
    public DbSet<BarbeariaSaaS.Domain.Entities.File> Files { get; set; }
    public DbSet<BusinessBreak> BusinessBreaks { get; set; }
    public DbSet<SpecialDay> SpecialDays { get; set; }
    public DbSet<ManualBlock> ManualBlocks { get; set; }
    public DbSet<TenantSetting> TenantSettings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Configurar warnings do EF Core
        optionsBuilder.ConfigureWarnings(warnings =>
        {
            // Em produção, suprimir warning de pending changes
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning);
            }
        });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Get database provider for conditional configurations
        var databaseProvider = Database.ProviderName;

        // Tenant configuration
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Subdomain).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Subdomain).IsUnique();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.Plan).HasConversion<int>();
            
            // Configure timestamps based on database provider
            if (databaseProvider?.Contains("SqlServer") == true)
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");
            }
            else if (databaseProvider?.Contains("Npgsql") == true)
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
            }
            else
            {
                // SQLite and others
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("datetime('now')");
            }

            // Complex type configurations for Branding and Settings as JSON
            entity.Property(e => e.Branding)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<TenantBranding>(v, (JsonSerializerOptions)null!)!)
                .HasColumnName("BrandingJson")
                .HasColumnType(databaseProvider?.Contains("SqlServer") == true ? "NVARCHAR(MAX)" : "TEXT");

            entity.Property(e => e.Settings)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<TenantSettings>(v, (JsonSerializerOptions)null!)!)
                .HasColumnName("SettingsJson")
                .HasColumnType(databaseProvider?.Contains("SqlServer") == true ? "NVARCHAR(MAX)" : "TEXT");

            entity.HasMany(t => t.BusinessBreaks)
                .WithOne(b => b.Tenant)
                .HasForeignKey(b => b.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(t => t.SpecialDays)
                .WithOne(s => s.Tenant)
                .HasForeignKey(s => s.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(t => t.ManualBlocks)
                .WithOne(m => m.Tenant)
                .HasForeignKey(m => m.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.TenantSetting)
                .WithOne(ts => ts.Tenant)
                .HasForeignKey<TenantSetting>(ts => ts.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Role).HasConversion<int>();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            // Configure timestamps
            if (databaseProvider?.Contains("SqlServer") == true)
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            }
            else if (databaseProvider?.Contains("Npgsql") == true)
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            }
            else
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            }

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Users)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.TenantId);
        });

        // Service configuration
        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Price).HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(e => e.DurationMinutes).IsRequired();
            entity.Property(e => e.Color).HasMaxLength(7);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            // Configure timestamps
            if (databaseProvider?.Contains("SqlServer") == true)
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            }
            else if (databaseProvider?.Contains("Npgsql") == true)
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            }
            else
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            }

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Services)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Services)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => new { e.TenantId, e.IsActive });
        });

        // Booking configuration
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.ServiceId).IsRequired();
            entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CustomerEmail).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CustomerPhone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.BookingDate).IsRequired().HasColumnType("DATE");
            entity.Property(e => e.BookingTime).IsRequired().HasColumnType("TIME");
            entity.Property(e => e.Status)
                .HasConversion<int>()
                .IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.ServicePrice).HasColumnType("decimal(10,2)").IsRequired();
            
            // Configure timestamps
            if (databaseProvider?.Contains("SqlServer") == true)
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");
            }
            else if (databaseProvider?.Contains("Npgsql") == true)
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
            }
            else
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("datetime('now')");
            }

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Bookings)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Service)
                .WithMany(s => s.Bookings)
                .HasForeignKey(e => e.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Bookings)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => new { e.TenantId, e.BookingDate });
            entity.HasIndex(e => new { e.TenantId, e.Status });
        });

        // Customer configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            
            // Configure timestamps
            if (databaseProvider?.Contains("SqlServer") == true)
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            }
            else if (databaseProvider?.Contains("Npgsql") == true)
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            }
            else
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            }

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Customers)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => new { e.TenantId, e.Email });
        });

        // ServiceCategory configuration
        modelBuilder.Entity<ServiceCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(300);
            entity.Property(e => e.Color).HasMaxLength(7);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.ServiceCategories)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // BusinessHour configuration
        modelBuilder.Entity<BusinessHour>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.DayOfWeek).IsRequired();
            entity.Property(e => e.IsOpen).HasDefaultValue(true);
            entity.Property(e => e.OpenTime).IsRequired().HasColumnType("TIME");
            entity.Property(e => e.CloseTime).IsRequired().HasColumnType("TIME");

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.BusinessHours)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // File configuration
        modelBuilder.Entity<BarbeariaSaaS.Domain.Entities.File>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FileSize).IsRequired();
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
            
            // Configure timestamps
            if (databaseProvider?.Contains("SqlServer") == true)
            {
                entity.Property(e => e.UploadedAt).HasDefaultValueSql("GETDATE()");
            }
            else if (databaseProvider?.Contains("Npgsql") == true)
            {
                entity.Property(e => e.UploadedAt).HasDefaultValueSql("NOW()");
            }
            else
            {
                entity.Property(e => e.UploadedAt).HasDefaultValueSql("datetime('now')");
            }

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Files)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // BusinessBreak configuration
        modelBuilder.Entity<BusinessBreak>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.StartTime).IsRequired().HasColumnType("TIME");
            entity.Property(e => e.EndTime).IsRequired().HasColumnType("TIME");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.AppliesToAllDays).HasDefaultValue(true);
            
            // Configure timestamps
            if (databaseProvider?.Contains("SqlServer") == true)
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            }
            else if (databaseProvider?.Contains("Npgsql") == true)
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            }
            else
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            }

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.BusinessBreaks)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.TenantId);
            
            // Check constraint
            entity.HasCheckConstraint("CK_BusinessBreak_Time", "StartTime < EndTime");
        });

        // SpecialDay configuration
        modelBuilder.Entity<SpecialDay>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.Date).IsRequired().HasColumnType("DATE");
            entity.Property(e => e.Type).HasConversion<int>().IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IsOpen).HasDefaultValue(false);
            entity.Property(e => e.CustomStartTime).HasColumnType("TIME");
            entity.Property(e => e.CustomEndTime).HasColumnType("TIME");
            
            // Configure timestamps
            if (databaseProvider?.Contains("SqlServer") == true)
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            }
            else if (databaseProvider?.Contains("Npgsql") == true)
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            }
            else
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            }

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.SpecialDays)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.TenantId, e.Date }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.Date });
            entity.HasIndex(e => e.Date);
            
            // Check constraint for custom hours
            entity.HasCheckConstraint("CK_SpecialDay_Hours", 
                "(IsOpen = 0) OR (IsOpen = 1 AND CustomStartTime IS NOT NULL AND CustomEndTime IS NOT NULL AND CustomStartTime < CustomEndTime)");
        });

        // ManualBlock configuration
        modelBuilder.Entity<ManualBlock>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.Date).IsRequired().HasColumnType("DATE");
            entity.Property(e => e.StartTime).HasColumnType("TIME");
            entity.Property(e => e.EndTime).HasColumnType("TIME");
            entity.Property(e => e.Type).HasConversion<int>().IsRequired();
            entity.Property(e => e.Reason).IsRequired().HasMaxLength(255);
            
            // Configure timestamps
            if (databaseProvider?.Contains("SqlServer") == true)
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            }
            else if (databaseProvider?.Contains("Npgsql") == true)
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            }
            else
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            }

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.ManualBlocks)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.TenantId, e.Date });
            entity.HasIndex(e => e.TenantId);
            
            // Check constraint for time blocks
            entity.HasCheckConstraint("CK_ManualBlock_Type", 
                "(Type = 2) OR (Type = 1 AND StartTime IS NOT NULL AND EndTime IS NOT NULL AND StartTime < EndTime)");
        });

        // TenantSetting configuration
        modelBuilder.Entity<TenantSetting>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.SlotDurationMinutes).HasDefaultValue(30);
            entity.Property(e => e.AdvanceBookingDays).HasDefaultValue(30);
            entity.Property(e => e.MaxBookingsPerDay).HasDefaultValue(50);
            entity.Property(e => e.BookingBufferMinutes).HasDefaultValue(0);
            entity.Property(e => e.Timezone).HasMaxLength(50).HasDefaultValue("America/Sao_Paulo");
            entity.Property(e => e.AutoConfirmBookings).HasDefaultValue(true);
            
            // Configure timestamps
            if (databaseProvider?.Contains("SqlServer") == true)
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");
            }
            else if (databaseProvider?.Contains("Npgsql") == true)
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
            }
            else
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("datetime('now')");
            }

            entity.HasOne(e => e.Tenant)
                .WithOne(t => t.TenantSetting)
                .HasForeignKey<TenantSetting>(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.TenantId).IsUnique();
            
            // Check constraints for positive values
            entity.HasCheckConstraint("CK_TenantSetting_SlotDuration", "SlotDurationMinutes > 0");
            entity.HasCheckConstraint("CK_TenantSetting_AdvanceDays", "AdvanceBookingDays > 0");
            entity.HasCheckConstraint("CK_TenantSetting_MaxBookings", "MaxBookingsPerDay > 0");
            entity.HasCheckConstraint("CK_TenantSetting_Buffer", "BookingBufferMinutes >= 0");
        });
    }
} 