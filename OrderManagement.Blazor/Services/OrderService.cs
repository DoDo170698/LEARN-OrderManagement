namespace OrderManagement.Blazor.Services;

/// <summary>
/// Service for managing orders via GraphQL API
/// </summary>
public class OrderService
{
    private readonly GraphQLClient _graphQLClient;

    public OrderService(GraphQLClient graphQLClient)
    {
        _graphQLClient = graphQLClient;
    }

    public async Task<List<Order>> GetAllOrdersAsync()
    {
        const string query = @"
            query {
                getOrders {
                    id
                    orderNumber
                    customerName
                    customerEmail
                    status
                    totalAmount
                    createdAt
                    updatedAt
                    items {
                        id
                        productName
                        quantity
                        unitPrice
                        subtotal
                    }
                }
            }";

        var response = await _graphQLClient.ExecuteQueryAsync<OrdersQueryResponse>(query);

        if (response.Errors != null && response.Errors.Any())
        {
            throw new Exception($"GraphQL Error: {response.Errors[0].Message}");
        }

        return response.Data?.GetOrders ?? new List<Order>();
    }

    public async Task<Order?> GetOrderByIdAsync(Guid id)
    {
        const string query = @"
            query($id: UUID!) {
                getOrderById(id: $id) {
                    id
                    orderNumber
                    customerName
                    customerEmail
                    status
                    totalAmount
                    createdAt
                    updatedAt
                    items {
                        id
                        productName
                        quantity
                        unitPrice
                        subtotal
                        createdAt
                    }
                }
            }";

        var variables = new { id };

        var response = await _graphQLClient.ExecuteQueryAsync<OrderByIdQueryResponse>(query, variables);

        if (response.Errors != null && response.Errors.Any())
        {
            throw new Exception($"GraphQL Error: {response.Errors[0].Message}");
        }

        return response.Data?.GetOrderById;
    }

    public async Task<Order> CreateOrderAsync(CreateOrderInput input)
    {
        const string mutation = @"
            mutation($input: CreateOrderInput!) {
                createOrder(input: $input) {
                    order {
                        id
                        orderNumber
                        customerName
                        customerEmail
                        status
                        totalAmount
                        createdAt
                        updatedAt
                        items {
                            id
                            productName
                            quantity
                            unitPrice
                            subtotal
                        }
                    }
                }
            }";

        var variables = new { input };

        var response = await _graphQLClient.ExecuteMutationAsync<CreateOrderMutationResponse>(mutation, variables);

        if (response.Errors != null && response.Errors.Any())
        {
            throw new Exception($"GraphQL Error: {response.Errors[0].Message}");
        }

        return response.Data?.CreateOrder?.Order ?? throw new Exception("Failed to create order");
    }

    public async Task<Order> UpdateOrderAsync(UpdateOrderInput input)
    {
        const string mutation = @"
            mutation($input: UpdateOrderInput!) {
                updateOrder(input: $input) {
                    order {
                        id
                        orderNumber
                        customerName
                        customerEmail
                        status
                        totalAmount
                        createdAt
                        updatedAt
                        items {
                            id
                            productName
                            quantity
                            unitPrice
                            subtotal
                        }
                    }
                }
            }";

        var variables = new { input };

        var response = await _graphQLClient.ExecuteMutationAsync<UpdateOrderMutationResponse>(mutation, variables);

        if (response.Errors != null && response.Errors.Any())
        {
            throw new Exception($"GraphQL Error: {response.Errors[0].Message}");
        }

        return response.Data?.UpdateOrder?.Order ?? throw new Exception("Failed to update order");
    }
}
