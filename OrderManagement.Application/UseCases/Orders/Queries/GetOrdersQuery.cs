using MediatR;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.UseCases.Orders.Queries;

public class GetOrdersQuery : IRequest<IQueryable<Order>>
{
}

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, IQueryable<Order>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetOrdersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<IQueryable<Order>> Handle(GetOrdersQuery query, CancellationToken cancellationToken = default)
    {
        var queryable = _unitOfWork.Orders.GetOrdersQueryable();

        return Task.FromResult(queryable);
    }
}
