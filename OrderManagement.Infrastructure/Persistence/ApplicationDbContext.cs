using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Persistence.Configurations;

namespace OrderManagement.Infrastructure.Persistence;

/// <summary>
/// Database context for the Order Management application
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Orders table
    /// </summary>
    public DbSet<Order> Orders => Set<Order>();

    /// <summary>
    /// Order items table
    /// </summary>
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // SQLite doesn't support DateTimeOffset natively, convert to DateTime (UTC)
        configurationBuilder.Properties<DateTimeOffset>()
            .HaveConversion<DateTimeOffsetToDateTimeConverter>();
    }

    /// <summary>
    /// Converter for SQLite: DateTimeOffset <-> DateTime (stores as UTC)
    /// </summary>
    private class DateTimeOffsetToDateTimeConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTimeOffset, DateTime>
    {
        public DateTimeOffsetToDateTimeConverter()
            : base(
                dateTimeOffset => dateTimeOffset.UtcDateTime,
                dateTime => new DateTimeOffset(dateTime, TimeSpan.Zero))
        {
        }
    }
}
