using OrderManagement.Application.DTOs;

namespace OrderManagement.GraphQL.GraphQL.Payloads;

/// <summary>
/// Payload returned after creating an order
/// </summary>
public record CreateOrderPayload(OrderDto Order);
