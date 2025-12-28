using MediatR;
using OrderManagement.Domain.Common;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.UseCases.Orders.Commands;

public class DeleteOrderCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
}

public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteOrderCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteOrderCommand command, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(command.Id, cancellationToken);

        if (order == null)
        {
            return ResultExtensions.NotFound<bool>("Order", command.Id);
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // Delete order (cascade delete will remove items)
            await _unitOfWork.Orders.DeleteAsync(order.Id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
