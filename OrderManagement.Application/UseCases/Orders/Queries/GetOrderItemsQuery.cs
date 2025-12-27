using MediatR;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.UseCases.Orders.Queries;

/// <summary>
/// Query to get order items as IQueryable (for GraphQL paging/filtering/sorting)
/// </summary>
public class GetOrderItemsQuery : IRequest<IQueryable<OrderItem>>
{
    public Guid OrderId { get; set; }
}

/// <summary>
/// Handler returns IQueryable to support GraphQL features
/// </summary>
public class GetOrderItemsQueryHandler : IRequestHandler<GetOrderItemsQuery, IQueryable<OrderItem>>
{
    private readonly IOrderItemRepository _repository;

    public GetOrderItemsQueryHandler(IOrderItemRepository repository)
    {
        _repository = repository;
    }

    public Task<IQueryable<OrderItem>> Handle(GetOrderItemsQuery query, CancellationToken cancellationToken = default)
    {
        // Return IQueryable filtered by OrderId
        // GraphQL will apply additional paging/filtering/sorting
        IQueryable<OrderItem> queryable = _repository.GetQueryable()
            .Where(item => item.OrderId == query.OrderId)
            .OrderBy(item => item.CreatedAt);

        return Task.FromResult(queryable);
    }
}
