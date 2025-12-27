using AutoMapper;
using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Exceptions;
using OrderManagement.Domain.Interfaces;

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
    public List<CreateOrderItemDto>? Items { get; set; }
}

/// <summary>
/// Handler for updating an existing order
/// </summary>
public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateOrderCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OrderDto> Handle(UpdateOrderCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(command.Id, cancellationToken);

            if (order == null)
            {
                throw new OrderNotFoundException(command.Id);
            }

            // Update basic properties (always update if provided)
            order.CustomerName = command.CustomerName ?? order.CustomerName;
            order.CustomerEmail = command.CustomerEmail ?? order.CustomerEmail;

            if (command.Status.HasValue)
            {
                order.Status = command.Status.Value;
            }

            // Update items if provided
            if (command.Items != null && command.Items.Any())
            {
                var existingItems = order.Items.ToList();
                var newItemsCount = command.Items.Count;

                // Update or add items
                for (int i = 0; i < newItemsCount; i++)
                {
                    var itemDto = command.Items[i];

                    if (i < existingItems.Count)
                    {
                        // Update existing item
                        var existingItem = existingItems[i];
                        existingItem.ProductName = itemDto.ProductName;
                        existingItem.Quantity = itemDto.Quantity;
                        existingItem.UnitPrice = itemDto.UnitPrice;
                        existingItem.Subtotal = itemDto.Quantity * itemDto.UnitPrice;
                    }
                    else
                    {
                        // Add new item
                        var newItem = new OrderItem
                        {
                            Id = Guid.NewGuid(),
                            OrderId = order.Id,
                            ProductName = itemDto.ProductName,
                            Quantity = itemDto.Quantity,
                            UnitPrice = itemDto.UnitPrice,
                            Subtotal = itemDto.Quantity * itemDto.UnitPrice,
                            CreatedAt = DateTime.UtcNow
                        };
                        order.Items.Add(newItem);
                    }
                }

                // Remove excess items if new list is shorter
                if (existingItems.Count > newItemsCount)
                {
                    for (int i = existingItems.Count - 1; i >= newItemsCount; i--)
                    {
                        order.Items.Remove(existingItems[i]);
                    }
                }
            }

            order.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // Reload order with items
            var updatedOrder = await _unitOfWork.Orders.GetOrderWithItemsAsync(order.Id, cancellationToken);
            var orderDto = _mapper.Map<OrderDto>(updatedOrder);

            return orderDto;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
