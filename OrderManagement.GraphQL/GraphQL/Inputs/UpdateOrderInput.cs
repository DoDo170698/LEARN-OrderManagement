using OrderManagement.Domain.Enums;

namespace OrderManagement.GraphQL.GraphQL.Inputs;

/// <summary>
/// Input for updating an existing order
/// </summary>
public record UpdateOrderInput(
    Guid Id,
    string? CustomerName,
    string? CustomerEmail,
    OrderStatus? Status,
    List<CreateOrderItemInput>? Items
);
