using Microsoft.AspNetCore.Components;
using OrderManagement.Blazor.GraphQL;
using OrderManagement.Blazor.Helpers;
using OrderManagement.Blazor.Resources.Pages.Orders;

namespace OrderManagement.Blazor.Pages.Orders;

/// <summary>
/// Code-behind for CreateOrder page
/// </summary>
public partial class CreateOrder
{
    [Inject] private IOrderManagementClient GraphQLClient { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private OrderInput orderInput = new()
    {
        Items = new() { new OrderItemInput() }
    };

    private bool isSubmitting = false;
    private string? errorMessage;

    private void AddItem()
    {
        orderInput.Items.Add(new OrderItemInput());
    }

    private void RemoveItem(int index)
    {
        if (orderInput.Items.Count > 1)
        {
            orderInput.Items.RemoveAt(index);
        }
    }

    private decimal CalculateTotal()
    {
        return orderInput.Items.Sum(i => i.Quantity * i.UnitPrice);
    }

    private void Cancel()
    {
        Navigation.NavigateTo("/orders");
    }

    private async Task HandleSubmit()
    {
        try
        {
            isSubmitting = true;
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(orderInput.CustomerName))
            {
                errorMessage = CreateOrderResources.CustomerNameRequired;
                return;
            }

            if (string.IsNullOrWhiteSpace(orderInput.CustomerEmail))
            {
                errorMessage = CreateOrderResources.CustomerEmailRequired;
                return;
            }

            if (orderInput.Items.Any(i => string.IsNullOrWhiteSpace(i.ProductName)))
            {
                errorMessage = CreateOrderResources.ProductNameRequired;
                return;
            }

            if (orderInput.Items.Any(i => i.Quantity <= 0))
            {
                errorMessage = CreateOrderResources.QuantityRequired;
                return;
            }

            if (orderInput.Items.Any(i => i.UnitPrice <= 0))
            {
                errorMessage = CreateOrderResources.UnitPriceRequired;
                return;
            }

            // Map to GraphQL input
            var input = new CreateOrderInput
            {
                CustomerName = orderInput.CustomerName,
                CustomerEmail = orderInput.CustomerEmail,
                Items = orderInput.Items.Select(i => new CreateOrderItemInput
                {
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            var result = await GraphQLClient.CreateOrder.ExecuteAsync(input);

            if (result.Data?.CreateOrder?.Order != null)
            {
                var orderId = result.Data.CreateOrder.Order.Id;
                Navigation.NavigateTo($"/orders/{orderId}");
            }
            else if (result.Errors?.Count > 0)
            {
                errorMessage = ErrorMessageHelper.GetErrorMessage(result);
            }
        }
        catch (Exception ex)
        {
            errorMessage = string.Format(CreateOrderResources.FailedToCreateOrder, ex.Message);
        }
        finally
        {
            isSubmitting = false;
        }
    }

    // Local input models for form binding
    public class OrderInput
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public List<OrderItemInput> Items { get; set; } = new();
    }

    public class OrderItemInput
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; } = 0;
    }
}
