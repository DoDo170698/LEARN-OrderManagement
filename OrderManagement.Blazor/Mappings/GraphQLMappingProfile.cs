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
        // Map from GetOrdersList query result (Connection with edges, NO items)
        CreateMap<IGetOrdersList_Orders_Edges_Node, OrderViewModel>();

        // Map from GetOrderDetails query result (includes items)
        CreateMap<IGetOrderDetails_OrderById, OrderViewModel>();
        CreateMap<IGetOrderDetails_OrderById_Items, OrderItemViewModel>();

        // Map from OnOrderCreated subscription (NO items)
        CreateMap<IOnOrderCreated_OnOrderCreated, OrderViewModel>();

        // Map from OnOrderUpdated subscription (NO items)
        CreateMap<IOnOrderUpdated_OnOrderUpdated, OrderViewModel>();
    }
}
