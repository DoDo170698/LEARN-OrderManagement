using OrderManagement.Blazor.GraphQL;

namespace OrderManagement.Blazor.ViewModels;

/// <summary>
/// View model for Order display in UI
/// </summary>
public class OrderViewModel
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    // REMOVED: TotalAmount - causes field cost exceeded (requires loading all items)
    // Use GetOrderDetails query to get totalAmount when needed
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<OrderItemViewModel> Items { get; set; } = new();
}
