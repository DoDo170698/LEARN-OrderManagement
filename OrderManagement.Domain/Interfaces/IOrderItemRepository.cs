using OrderManagement.Domain.Entities;

namespace OrderManagement.Domain.Interfaces;

/// <summary>
/// Repository interface for OrderItem entity with specific operations
/// </summary>
public interface IOrderItemRepository : IRepository<OrderItem>
{
    Task<IEnumerable<OrderItem>> GetItemsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task DeleteItemsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
}
