namespace OrderManagement.GraphQL.GraphQL.Inputs;

/// <summary>
/// Input for creating a new order
/// </summary>
public record CreateOrderInput(
    string CustomerName,
    string CustomerEmail,
    List<CreateOrderItemInput> Items
);
