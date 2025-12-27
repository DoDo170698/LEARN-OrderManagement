namespace OrderManagement.Blazor.ViewModels;

/// <summary>
/// View model for OrderItem display in UI
/// </summary>
public class OrderItemViewModel
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}
