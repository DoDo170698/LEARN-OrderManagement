using MediatR;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.UseCases.Orders.Queries;

/// <summary>
/// Query to get orders as IQueryable (for GraphQL paging/filtering/sorting)
/// </summary>
public class GetOrdersQuery : IRequest<IQueryable<Order>>
{
    // No parameters - returns IQueryable for client-side filtering
}

/// <summary>
/// Handler returns IQueryable to support GraphQL features
/// </summary>
public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, IQueryable<Order>>
{
    private readonly IOrderRepository _repository;

    public GetOrdersQueryHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public Task<IQueryable<Order>> Handle(GetOrdersQuery query, CancellationToken cancellationToken = default)
    {
        // Return IQueryable - NOT materialized
        // GraphQL will apply paging/filtering/sorting
        var queryable = _repository.GetOrdersQueryable();

        return Task.FromResult(queryable);
    }
}
