using MediatR;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<DeleteOrderCommandHandler> _logger;

    public DeleteOrderCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteOrderCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete order {OrderId}", command.Id);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
