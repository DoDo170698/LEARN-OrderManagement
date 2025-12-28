using Microsoft.AspNetCore.Components;
using OrderManagement.Blazor.GraphQL;
using OrderManagement.Blazor.Helpers;
using OrderManagement.Blazor.Resources.Pages.Orders;

namespace OrderManagement.Blazor.Pages.Orders;

/// <summary>
/// Code-behind for OrderDetail page
/// </summary>
public partial class OrderDetail
{
    [Inject] private IOrderManagementClient GraphQLClient { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Parameter]
    public Guid Id { get; set; }

    private IGetOrderDetails_OrderById? order;
    private bool isLoading = true;
    private string? errorMessage;
    private bool showDeleteConfirmation = false;
    private bool isDeleting = false;

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
                // Success - order returned directly (GraphQL standard)
                order = result.Data.OrderById;
            }
            else if (result.Errors?.Count > 0)
            {
                // GraphQL standard - errors in top-level errors array
                errorMessage = ErrorMessageHelper.GetErrorMessage(result);
            }
            else
            {
                errorMessage = OrderDetailResources.OrderNotFound;
            }
        }
        catch (Exception ex)
        {
            errorMessage = string.Format(OrderDetailResources.FailedToLoadOrder, ex.Message);
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task RefreshAsync()
    {
        await LoadOrderAsync();
    }

    private string GetStatusBadgeClass(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => "bg-warning text-dark",
            OrderStatus.Processing => "bg-info text-white",
            OrderStatus.Completed => "bg-success text-white",
            OrderStatus.Cancelled => "bg-danger text-white",
            _ => "bg-secondary text-white"
        };
    }

    private void ShowDeleteConfirmation()
    {
        showDeleteConfirmation = true;
    }

    private void CancelDelete()
    {
        showDeleteConfirmation = false;
    }

    private async Task ConfirmDelete()
    {
        try
        {
            isDeleting = true;
            errorMessage = null;

            var result = await GraphQLClient.DeleteOrder.ExecuteAsync(Id);

            if (result.Data?.DeleteOrder == true)
            {
                // Navigate back to orders list after successful delete
                Navigation.NavigateTo("/orders");
            }
            else if (result.Errors?.Count > 0)
            {
                // GraphQL standard - errors in top-level errors array
                errorMessage = ErrorMessageHelper.GetErrorMessage(result);
                showDeleteConfirmation = false;
            }
            else
            {
                errorMessage = OrderDetailResources.FailedToDeleteOrderGeneric;
                showDeleteConfirmation = false;
            }
        }
        catch (Exception ex)
        {
            errorMessage = string.Format(OrderDetailResources.FailedToDeleteOrder, ex.Message);
            showDeleteConfirmation = false;
        }
        finally
        {
            isDeleting = false;
        }
    }
}
