using AutoMapper;
using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.UseCases.Orders.Commands;

/// <summary>
/// Command to create a new order
/// </summary>
public class CreateOrderCommand : IRequest<OrderDto>
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public List<CreateOrderItemDto> Items { get; set; } = new();
}

/// <summary>
/// Handler for creating a new order
/// </summary>
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateOrderCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // Generate order number
            var orderNumber = await GenerateOrderNumberAsync(cancellationToken);

            // Create order entity
            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = orderNumber,
                CustomerName = command.CustomerName,
                CustomerEmail = command.CustomerEmail,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Orders.AddAsync(order, cancellationToken);

            // Create order items
            foreach (var itemDto in command.Items)
            {
                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductName = itemDto.ProductName,
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice,
                    Subtotal = itemDto.Quantity * itemDto.UnitPrice,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.OrderItems.AddAsync(orderItem, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // Reload order with items
            var createdOrder = await _unitOfWork.Orders.GetOrderWithItemsAsync(order.Id, cancellationToken);
            var orderDto = _mapper.Map<OrderDto>(createdOrder);

            return orderDto;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _unitOfWork.Orders.GetOrderCountByYearAsync(year, cancellationToken);
        var nextNumber = count + 1;
        return $"ORD-{year}-{nextNumber:D3}";
    }
}
