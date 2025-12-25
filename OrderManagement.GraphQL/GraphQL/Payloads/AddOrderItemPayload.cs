using OrderManagement.Application.DTOs;

namespace OrderManagement.GraphQL.GraphQL.Payloads;

/// <summary>
/// Payload returned after adding an item to an order
/// </summary>
public record AddOrderItemPayload(OrderItemDto? OrderItem);
