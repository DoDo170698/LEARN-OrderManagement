using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.UseCases.Orders.Commands;

/// <summary>
/// Command to update an existing order
/// </summary>
public class UpdateOrderCommand : IRequest<OrderDto>
{
    public Guid Id { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public OrderStatus? Status { get; set; }
}
