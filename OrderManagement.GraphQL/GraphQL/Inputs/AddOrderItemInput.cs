namespace OrderManagement.GraphQL.GraphQL.Inputs;

/// <summary>
/// Input for adding an item to an existing order
/// </summary>
public record AddOrderItemInput(
    Guid OrderId,
    string ProductName,
    int Quantity,
    decimal UnitPrice
);
