using Microsoft.AspNetCore.Components;
using OrderManagement.Blazor.GraphQL;
using OrderManagement.Blazor.Resources.Pages.Orders;

namespace OrderManagement.Blazor.Pages.Orders;

/// <summary>
/// Code-behind for EditOrder page
/// </summary>
public partial class EditOrder
{
    [Inject] private IOrderManagementClient GraphQLClient { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Parameter]
    public Guid Id { get; set; }

    private IGetOrderDetails_OrderById? order;
    private OrderInput orderInput = new();
    private string? selectedStatus;

    private bool isLoading = true;
    private bool isSubmitting = false;
    private string? errorMessage;

    protected override async Task OnInitializedAsync()
    {
        await LoadOrderAsync();
    }

    private async Task LoadOrderAsync()
    {
        try
        {
            isLoading = true;
            errorMessage = null;

            var result = await GraphQLClient.GetOrderDetails.ExecuteAsync(Id);

            if (result.Data?.OrderById != null)
            {
                order = result.Data.OrderById;

                // Populate form with current values
                orderInput.CustomerName = order.CustomerName;
                orderInput.CustomerEmail = order.CustomerEmail;
                selectedStatus = order.Status.ToString();

                // Load existing items
                orderInput.Items = order.Items.Select(i => new OrderItemInput
                {
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList();

                // Ensure at least one item
                if (!orderInput.Items.Any())
                {
                    orderInput.Items.Add(new OrderItemInput());
                }
            }
            else if (result.Errors?.Count > 0)
            {
                errorMessage = string.Join(", ", result.Errors.Select(e => e.Message));
            }
        }
        catch (Exception ex)
        {
            errorMessage = string.Format(EditOrderResources.FailedToLoadOrder, ex.Message);
        }
        finally
        {
            isLoading = false;
        }
    }

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
        Navigation.NavigateTo($"/orders/{Id}");
    }

    private async Task HandleSubmit()
    {
        try
        {
            isSubmitting = true;
            errorMessage = null;

            if (order == null)
            {
                errorMessage = EditOrderResources.OrderDataNotLoaded;
                return;
            }

            // Validation
            if (string.IsNullOrWhiteSpace(orderInput.CustomerName))
            {
                errorMessage = EditOrderResources.CustomerNameRequired;
                return;
            }

            if (string.IsNullOrWhiteSpace(orderInput.CustomerEmail))
            {
                errorMessage = EditOrderResources.CustomerEmailRequired;
                return;
            }

            if (orderInput.Items.Any(i => string.IsNullOrWhiteSpace(i.ProductName)))
            {
                errorMessage = EditOrderResources.ProductNameRequired;
                return;
            }

            if (orderInput.Items.Any(i => i.Quantity <= 0))
            {
                errorMessage = EditOrderResources.QuantityRequired;
                return;
            }

            if (orderInput.Items.Any(i => i.UnitPrice <= 0))
            {
                errorMessage = EditOrderResources.UnitPriceRequired;
                return;
            }

            // Parse status
            OrderStatus? parsedStatus = null;
            if (!string.IsNullOrEmpty(selectedStatus))
            {
                parsedStatus = Enum.Parse<OrderStatus>(selectedStatus);
            }

            // Map to GraphQL input
            var input = new UpdateOrderInput
            {
                Id = Id,
                CustomerName = orderInput.CustomerName,
                CustomerEmail = orderInput.CustomerEmail,
                Status = parsedStatus,
                Items = orderInput.Items.Select(i => new CreateOrderItemInput
                {
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            var result = await GraphQLClient.UpdateOrder.ExecuteAsync(input);

            if (result.Data?.UpdateOrder?.Order != null)
            {
                // Navigate to detail page
                Navigation.NavigateTo($"/orders/{Id}");
            }
            else if (result.Errors?.Count > 0)
            {
                errorMessage = string.Join(", ", result.Errors.Select(e => e.Message));
            }
        }
        catch (Exception ex)
        {
            errorMessage = string.Format(EditOrderResources.FailedToUpdateOrder, ex.Message);
        }
        finally
        {
            isSubmitting = false;
        }
    }

    // Local input models for form binding (same as CreateOrder)
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
