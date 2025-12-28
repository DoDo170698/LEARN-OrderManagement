using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Domain.Interfaces;

/// <summary>
/// Repository interface for Order entity with specific operations
/// </summary>
public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetOrderWithItemsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get orders queryable WITHOUT items (optimized for list view)
    /// </summary>
    IQueryable<Order> GetOrdersQueryable();

    /// <summary>
    /// PERFORMANCE FIX: Get count of orders for specific year without loading all data
    /// </summary>
    Task<int> GetOrderCountByYearAsync(int year, CancellationToken cancellationToken = default);
}
