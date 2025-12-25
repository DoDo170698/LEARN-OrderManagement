using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity configuration for Order entity
/// </summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(o => o.OrderNumber)
            .IsUnique();

        builder.Property(o => o.CustomerName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.CustomerEmail)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .IsRequired();

        // Relationship configuration
        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed data
        builder.HasData(
            new Order
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                OrderNumber = "ORD-2025-001",
                CustomerName = "Nguyen Van A",
                CustomerEmail = "nguyenvana@example.com",
                Status = OrderStatus.Completed,
                CreatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 1, 14, 30, 0, DateTimeKind.Utc)
            },
            new Order
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                OrderNumber = "ORD-2025-002",
                CustomerName = "Tran Thi B",
                CustomerEmail = "tranthib@example.com",
                Status = OrderStatus.Processing,
                CreatedAt = new DateTime(2025, 1, 2, 9, 15, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 2, 9, 15, 0, DateTimeKind.Utc)
            },
            new Order
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                OrderNumber = "ORD-2025-003",
                CustomerName = "Le Van C",
                CustomerEmail = "levanc@example.com",
                Status = OrderStatus.Pending,
                CreatedAt = new DateTime(2025, 1, 3, 11, 45, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 3, 11, 45, 0, DateTimeKind.Utc)
            }
        );
    }
}
