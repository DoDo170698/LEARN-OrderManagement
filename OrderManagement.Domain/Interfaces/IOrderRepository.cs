using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Domain.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetOrderWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    IQueryable<Order> GetOrdersQueryable();
    Task<int> GetOrderCountByYearAsync(int year, CancellationToken cancellationToken = default);
}
