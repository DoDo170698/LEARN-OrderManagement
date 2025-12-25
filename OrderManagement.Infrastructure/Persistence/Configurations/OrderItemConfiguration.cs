using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity configuration for OrderItem entity
/// </summary>
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Quantity)
            .IsRequired();

        builder.Property(i => i.UnitPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(i => i.Subtotal)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(i => i.CreatedAt)
            .IsRequired();

        // Seed data
        builder.HasData(
            // Items for Order 1 (ORD-2025-001)
            new OrderItem
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                OrderId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                ProductName = "Laptop Dell XPS 15",
                Quantity = 1,
                UnitPrice = 1500.00m,
                Subtotal = 1500.00m,
                CreatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc)
            },
            new OrderItem
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                OrderId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                ProductName = "Wireless Mouse Logitech MX Master 3",
                Quantity = 2,
                UnitPrice = 99.99m,
                Subtotal = 199.98m,
                CreatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc)
            },

            // Items for Order 2 (ORD-2025-002)
            new OrderItem
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                OrderId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                ProductName = "iPhone 15 Pro Max",
                Quantity = 1,
                UnitPrice = 1199.00m,
                Subtotal = 1199.00m,
                CreatedAt = new DateTime(2025, 1, 2, 9, 15, 0, DateTimeKind.Utc)
            },
            new OrderItem
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                OrderId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                ProductName = "AirPods Pro 2",
                Quantity = 1,
                UnitPrice = 249.00m,
                Subtotal = 249.00m,
                CreatedAt = new DateTime(2025, 1, 2, 9, 15, 0, DateTimeKind.Utc)
            },

            // Items for Order 3 (ORD-2025-003)
            new OrderItem
            {
                Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                OrderId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                ProductName = "Samsung 4K Monitor 32\"",
                Quantity = 2,
                UnitPrice = 450.00m,
                Subtotal = 900.00m,
                CreatedAt = new DateTime(2025, 1, 3, 11, 45, 0, DateTimeKind.Utc)
            }
        );
    }
}
