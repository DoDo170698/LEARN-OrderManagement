using OrderManagement.Application.DTOs;

namespace OrderManagement.GraphQL.GraphQL.Payloads;

/// <summary>
/// Payload returned after updating an order
/// </summary>
public record UpdateOrderPayload(OrderDto? Order);
