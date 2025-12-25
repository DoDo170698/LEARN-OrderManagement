namespace OrderManagement.GraphQL.GraphQL.Payloads;

/// <summary>
/// Error returned when an order is not found
/// </summary>
public class OrderNotFoundError : Exception
{
    public Guid OrderId { get; }

    public OrderNotFoundError(Guid orderId)
        : base($"Order with ID '{orderId}' was not found.")
    {
        OrderId = orderId;
    }
}
