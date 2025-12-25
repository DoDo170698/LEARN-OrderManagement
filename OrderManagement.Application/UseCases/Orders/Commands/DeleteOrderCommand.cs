using MediatR;
using OrderManagement.Domain.Exceptions;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.UseCases.Orders.Commands;

public class DeleteOrderCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteOrderCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteOrderCommand command, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(command.Id, cancellationToken);

        if (order == null)
        {
            throw new OrderNotFoundException(command.Id);
        }

        // Business rule: Cannot delete completed orders
        if (order.Status == Domain.Enums.OrderStatus.Completed)
        {
            throw new InvalidOperationException("Cannot delete a completed order");
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Delete order items first (cascade delete will handle this, but explicit is better)
            foreach (var item in order.Items)
            {
                await _unitOfWork.OrderItems.DeleteAsync(item.Id, cancellationToken);
            }

            await _unitOfWork.Orders.DeleteAsync(command.Id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
