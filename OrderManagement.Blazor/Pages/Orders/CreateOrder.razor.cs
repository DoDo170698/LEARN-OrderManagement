using Microsoft.AspNetCore.Components;
using OrderManagement.Blazor.GraphQL;
using OrderManagement.Blazor.Helpers;
using OrderManagement.Blazor.Resources.Pages.Orders;

namespace OrderManagement.Blazor.Pages.Orders;

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
    private Dictionary<string, string> fieldErrors = new();

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

            if (result.Data?.CreateOrder != null)
            {
                var orderId = result.Data.CreateOrder.Id;
                Navigation.NavigateTo($"/orders/{orderId}");
            }
            else if (result.Errors?.Count > 0)
            {
                errorMessage = ErrorMessageHelper.GetErrorMessage(result);
                fieldErrors.Clear();
                var firstError = result.Errors.First();
                if (firstError.Extensions != null && firstError.Extensions.ContainsKey("fields"))
                {
                    var fields = firstError.Extensions["fields"] as Dictionary<string, object>;
                    if (fields != null)
                    {
                        foreach (var field in fields)
                        {
                            if (field.Value is string[] messages && messages.Length > 0)
                            {
                                fieldErrors[field.Key] = messages[0];
                            }
                            else if (field.Value is string message)
                            {
                                fieldErrors[field.Key] = message;
                            }
                        }
                    }
                }
            }
            else
            {
                errorMessage = "Failed to create order. Please try again.";
            }
        }
        catch (HttpRequestException ex)
        {
            errorMessage = ErrorMessageHelper.HandleException(ex, "CreateOrder");
        }
        catch (OperationCanceledException ex)
        {
            errorMessage = ErrorMessageHelper.HandleException(ex, "CreateOrder");
        }
        catch (Exception ex)
        {
            errorMessage = ErrorMessageHelper.HandleException(ex, "CreateOrder");
        }
        finally
        {
            isSubmitting = false;
        }
    }

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
