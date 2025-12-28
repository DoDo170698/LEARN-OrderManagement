namespace OrderManagement.Domain.Entities;

/// <summary>
/// Represents an item within an order
/// </summary>
public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public virtual Order Order { get; set; } = null!;
}
