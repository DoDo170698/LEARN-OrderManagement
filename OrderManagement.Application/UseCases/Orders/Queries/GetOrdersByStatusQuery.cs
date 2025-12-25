using AutoMapper;
using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.UseCases.Orders.Queries;

public class GetOrdersByStatusQuery : IRequest<List<OrderDto>>
{
    public OrderStatus Status { get; set; }
}

public class GetOrdersByStatusQueryHandler : IRequestHandler<GetOrdersByStatusQuery, List<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetOrdersByStatusQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<OrderDto>> Handle(GetOrdersByStatusQuery query, CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.GetOrdersByStatusAsync(query.Status, cancellationToken);
        var orderDtos = _mapper.Map<List<OrderDto>>(orders);

        return orderDtos;
    }
}
