using AutoMapper;
using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Domain.Common;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.UseCases.Orders.Queries;

public class GetOrderByIdQuery : IRequest<Result<OrderDto>>
{
    public Guid Id { get; set; }
}

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetOrderByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(query.Id, cancellationToken);

        if (order == null)
        {
            return ResultExtensions.NotFound<OrderDto>("Order", query.Id);
        }

        var orderDto = _mapper.Map<OrderDto>(order);

        return Result<OrderDto>.Success(orderDto);
    }
}
