namespace OrderManagement.GraphQL.GraphQL.Inputs;

/// <summary>
/// Input for creating an order item
/// </summary>
public record CreateOrderItemInput(
    string ProductName,
    int Quantity,
    decimal UnitPrice
);
