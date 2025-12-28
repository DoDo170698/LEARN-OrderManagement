using OrderManagement.Domain.Entities;

namespace OrderManagement.Domain.Interfaces;

/// <summary>
/// Repository interface for OrderItem entity with specific operations
/// </summary>
public interface IOrderItemRepository : IRepository<OrderItem>
{
}
