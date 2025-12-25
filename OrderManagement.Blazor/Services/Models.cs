using System.Text.Json.Serialization;

namespace OrderManagement.Blazor.Services;

public enum OrderStatus
{
    Pending,
    Processing,
    Completed,
    Cancelled
}

public class Order
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("orderNumber")]
    public string OrderNumber { get; set; } = string.Empty;

    [JsonPropertyName("customerName")]
    public string CustomerName { get; set; } = string.Empty;

    [JsonPropertyName("customerEmail")]
    public string CustomerEmail { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public OrderStatus Status { get; set; }

    [JsonPropertyName("totalAmount")]
    public decimal TotalAmount { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("items")]
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("orderId")]
    public Guid OrderId { get; set; }

    [JsonPropertyName("productName")]
    public string ProductName { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("unitPrice")]
    public decimal UnitPrice { get; set; }

    [JsonPropertyName("subtotal")]
    public decimal Subtotal { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}

// Response wrappers for GraphQL queries
public class OrdersQueryResponse
{
    [JsonPropertyName("getOrders")]
    public List<Order> GetOrders { get; set; } = new();
}

public class OrderByIdQueryResponse
{
    [JsonPropertyName("getOrderById")]
    public Order? GetOrderById { get; set; }
}

public class CreateOrderMutationResponse
{
    [JsonPropertyName("createOrder")]
    public CreateOrderPayload? CreateOrder { get; set; }
}

public class UpdateOrderMutationResponse
{
    [JsonPropertyName("updateOrder")]
    public UpdateOrderPayload? UpdateOrder { get; set; }
}

public class CreateOrderPayload
{
    [JsonPropertyName("order")]
    public Order? Order { get; set; }
}

public class UpdateOrderPayload
{
    [JsonPropertyName("order")]
    public Order? Order { get; set; }
}

// Input types for mutations
public class CreateOrderInput
{
    [JsonPropertyName("customerName")]
    public string CustomerName { get; set; } = string.Empty;

    [JsonPropertyName("customerEmail")]
    public string CustomerEmail { get; set; } = string.Empty;

    [JsonPropertyName("items")]
    public List<CreateOrderItemInput> Items { get; set; } = new();
}

public class CreateOrderItemInput
{
    [JsonPropertyName("productName")]
    public string ProductName { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("unitPrice")]
    public decimal UnitPrice { get; set; }
}

public class UpdateOrderInput
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("customerName")]
    public string? CustomerName { get; set; }

    [JsonPropertyName("customerEmail")]
    public string? CustomerEmail { get; set; }

    [JsonPropertyName("status")]
    public OrderStatus? Status { get; set; }
}
