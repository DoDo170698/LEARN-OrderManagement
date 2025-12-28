using AutoMapper;
using OrderManagement.Blazor.GraphQL;
using OrderManagement.Blazor.ViewModels;

namespace OrderManagement.Blazor.Mappings;

/// <summary>
/// AutoMapper profile for mapping GraphQL result types to ViewModels
/// </summary>
public class GraphQLMappingProfile : Profile
{
    public GraphQLMappingProfile()
    {
        CreateMap<IGetOrdersList_Orders_Edges_Node, OrderViewModel>();
        CreateMap<IGetOrderDetails_OrderById, OrderViewModel>();
        CreateMap<IGetOrderDetails_OrderById_Items, OrderItemViewModel>();
        CreateMap<IOnOrderCreated_OnOrderCreated, OrderViewModel>();
        CreateMap<IOnOrderUpdated_OnOrderUpdated, OrderViewModel>();
    }
}
