using AutoMapper;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.UseCases.Orders.Commands;
using OrderManagement.GraphQL.GraphQL.Inputs;

namespace OrderManagement.GraphQL.Mappings;

/// <summary>
/// AutoMapper profile for mapping GraphQL Inputs to Commands/DTOs
/// </summary>
public class GraphQLInputMappingProfile : Profile
{
    public GraphQLInputMappingProfile()
    {
        // Map CreateOrderInput -> CreateOrderCommand
        CreateMap<CreateOrderInput, CreateOrderCommand>();

        // Map CreateOrderItemInput -> CreateOrderItemDto
        CreateMap<CreateOrderItemInput, CreateOrderItemDto>();

        // Map UpdateOrderInput -> UpdateOrderCommand
        CreateMap<UpdateOrderInput, UpdateOrderCommand>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));
    }
}
