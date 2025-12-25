using AutoMapper;
using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.UseCases.Orders.Queries;

/// <summary>
/// Query to get all orders
/// </summary>
public class GetAllOrdersQuery : IRequest<List<OrderDto>>
{
    // No parameters needed for getting all
}

public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, List<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllOrdersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<OrderDto>> Handle(GetAllOrdersQuery query, CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.GetAllOrdersWithItemsAsync(cancellationToken);
        var orderDtos = _mapper.Map<List<OrderDto>>(orders);

        return orderDtos;
    }
}
