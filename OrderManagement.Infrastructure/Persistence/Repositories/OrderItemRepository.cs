using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for OrderItem entity
/// </summary>
public class OrderItemRepository : Repository<OrderItem>, IOrderItemRepository
{
    public OrderItemRepository(ApplicationDbContext context) : base(context)
    {
    }
}
