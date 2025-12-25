using AutoMapper;
using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Domain.Exceptions;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.UseCases.Orders.Commands;

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
        var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(command.Id, cancellationToken);

        if (order == null)
        {
            throw new OrderNotFoundException(command.Id);
        }

        if (!string.IsNullOrWhiteSpace(command.CustomerName))
        {
            order.CustomerName = command.CustomerName;
        }

        if (!string.IsNullOrWhiteSpace(command.CustomerEmail))
        {
            order.CustomerEmail = command.CustomerEmail;
        }

        if (command.Status.HasValue)
        {
            order.Status = command.Status.Value;
        }

        order.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var orderDto = _mapper.Map<OrderDto>(order);

        return orderDto;
    }
}
