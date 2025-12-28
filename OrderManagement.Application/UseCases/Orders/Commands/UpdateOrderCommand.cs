using AutoMapper;
using FluentValidation;
using MediatR;
using OrderManagement.Application.Common.Extensions;
using OrderManagement.Application.DTOs;
using OrderManagement.Domain.Common;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.UseCases.Orders.Commands;

public class UpdateOrderCommand : IRequest<Result<OrderDto>>
{
    public Guid Id { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public OrderStatus? Status { get; set; }
    public List<CreateOrderItemDto>? Items { get; set; }
}

public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, Result<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateOrderCommand> _validator;

    public UpdateOrderCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<UpdateOrderCommand> validator)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<Result<OrderDto>> Handle(UpdateOrderCommand command, CancellationToken cancellationToken = default)
    {
        // Validate command
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResult<OrderDto>();
        }

            var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(command.Id, cancellationToken);

            if (order == null)
            {
                return ResultExtensions.NotFound<OrderDto>("Order", command.Id);
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

            order.CustomerName = command.CustomerName ?? order.CustomerName;
            order.CustomerEmail = command.CustomerEmail ?? order.CustomerEmail;

            if (command.Status.HasValue)
            {
                order.Status = command.Status.Value;
            }

            if (command.Items != null && command.Items.Any())
            {
                var existingItems = order.Items.ToList();
                var newItemsCount = command.Items.Count;

                for (int i = 0; i < newItemsCount; i++)
                {
                    var itemDto = command.Items[i];

                    if (i < existingItems.Count)
                    {
                        var existingItem = existingItems[i];
                        existingItem.ProductName = itemDto.ProductName;
                        existingItem.Quantity = itemDto.Quantity;
                        existingItem.UnitPrice = itemDto.UnitPrice;
                        existingItem.Subtotal = itemDto.Quantity * itemDto.UnitPrice;
                    }
                    else
                    {
                        var newItem = new OrderItem
                        {
                            Id = Guid.NewGuid(),
                            OrderId = order.Id,
                            ProductName = itemDto.ProductName,
                            Quantity = itemDto.Quantity,
                            UnitPrice = itemDto.UnitPrice,
                            Subtotal = itemDto.Quantity * itemDto.UnitPrice,
                            CreatedAt = DateTimeOffset.UtcNow
                        };
                        order.Items.Add(newItem);
                    }
                }

                if (existingItems.Count > newItemsCount)
                {
                    for (int i = existingItems.Count - 1; i >= newItemsCount; i--)
                    {
                        order.Items.Remove(existingItems[i]);
                    }
                }
            }

            order.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var updatedOrder = await _unitOfWork.Orders.GetOrderWithItemsAsync(order.Id, cancellationToken);
            var orderDto = _mapper.Map<OrderDto>(updatedOrder);

            return Result<OrderDto>.Success(orderDto);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
