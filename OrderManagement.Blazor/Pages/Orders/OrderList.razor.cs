using AutoMapper;
using Microsoft.AspNetCore.Components;
using OrderManagement.Blazor.GraphQL;
using OrderManagement.Blazor.Helpers;
using OrderManagement.Blazor.ViewModels;
using OrderManagement.Blazor.Resources.Pages.Orders;

namespace OrderManagement.Blazor.Pages.Orders;

/// <summary>
/// Code-behind for OrderList page
/// </summary>
public partial class OrderList : IDisposable
{
    [Inject] private IOrderManagementClient GraphQLClient { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IMapper Mapper { get; set; } = default!;

    private List<OrderViewModel> orders = new();
    private bool isLoading = true;
    private string? errorMessage;
    private bool subscriptionsActive = false;

    private IDisposable? _createSubscription;
    private IDisposable? _updateSubscription;
    private IDisposable? _deleteSubscription;

    private int pageSize = 10;
    private string? currentCursor = null;
    private string? startCursor = null;
    private string? endCursor = null;
    private bool hasNextPage = false;
    private bool hasPreviousPage = false;
    private Stack<string> cursorHistory = new();
    private int currentPageNumber = 1;
    private int totalCount = 0;

    private OrderStatus? filterStatus = null;
    private string searchText = string.Empty;

    private string sortField = "CREATED_AT";
    private bool sortDescending = true;

    private bool showDeleteConfirmation = false;
    private bool isDeleting = false;
    private Guid deleteOrderId;
    private string deleteOrderNumber = string.Empty;
    private string deleteCustomerName = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadOrdersAsync();
        SetupSubscriptions();
    }

    private async Task LoadOrdersAsync()
    {
        try
        {
            isLoading = true;
            errorMessage = null;

            var whereFilter = BuildFilter();
            var orderSort = BuildSort();

            var result = await GraphQLClient.GetOrdersList.ExecuteAsync(
                first: pageSize,
                after: currentCursor,
                last: null,
                before: null,
                where: whereFilter,
                order: orderSort,
                CancellationToken.None
            );

            if (result.Data?.Orders != null)
            {
                orders = result.Data.Orders.Edges?
                    .Select(edge => Mapper.Map<OrderViewModel>(edge.Node))
                    .ToList() ?? new List<OrderViewModel>();

                // Update pagination state
                totalCount = result.Data.Orders.TotalCount;
                hasNextPage = result.Data.Orders.PageInfo?.HasNextPage ?? false;
                hasPreviousPage = result.Data.Orders.PageInfo?.HasPreviousPage ?? false;
                startCursor = result.Data.Orders.PageInfo?.StartCursor;
                endCursor = result.Data.Orders.PageInfo?.EndCursor;
            }
            else if (result.Errors?.Count > 0)
            {
                errorMessage = ErrorMessageHelper.GetErrorMessage(result);
            }
        }
        catch (HttpRequestException ex)
        {
            errorMessage = ErrorMessageHelper.HandleException(ex, "LoadOrders");
        }
        catch (OperationCanceledException ex)
        {
            errorMessage = ErrorMessageHelper.HandleException(ex, "LoadOrders");
        }
        catch (Exception ex)
        {
            errorMessage = ErrorMessageHelper.HandleException(ex, "LoadOrders");
        }
        finally
        {
            isLoading = false;
        }
    }

    private OrderFilterInput? BuildFilter()
    {
        var filters = new List<OrderFilterInput>();

        // Filter by status
        if (filterStatus.HasValue)
        {
            filters.Add(new OrderFilterInput
            {
                Status = new OrderStatusOperationFilterInput
                {
                    Eq = filterStatus.Value
                }
            });
        }

        // Filter by search text (customer name or email or order number)
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            filters.Add(new OrderFilterInput
            {
                Or = new[]
                {
                    new OrderFilterInput
                    {
                        CustomerName = new StringOperationFilterInput
                        {
                            Contains = searchText
                        }
                    },
                    new OrderFilterInput
                    {
                        CustomerEmail = new StringOperationFilterInput
                        {
                            Contains = searchText
                        }
                    },
                    new OrderFilterInput
                    {
                        OrderNumber = new StringOperationFilterInput
                        {
                            Contains = searchText
                        }
                    }
                }
            });
        }

        return filters.Count > 0 ? new OrderFilterInput { And = filters.ToArray() } : null;
    }

    private IReadOnlyList<OrderSortInput>? BuildSort()
    {
        var sortDirection = sortDescending ? SortEnumType.Desc : SortEnumType.Asc;
        var sortInput = sortField switch
        {
            "ORDER_NUMBER" => new OrderSortInput { OrderNumber = sortDirection },
            "CUSTOMER_NAME" => new OrderSortInput { CustomerName = sortDirection },
            "CUSTOMER_EMAIL" => new OrderSortInput { CustomerEmail = sortDirection },
            "TOTAL_AMOUNT" => new OrderSortInput { TotalAmount = sortDirection },
            "STATUS" => new OrderSortInput { Status = sortDirection },
            "CREATED_AT" => new OrderSortInput { CreatedAt = sortDirection },
            _ => new OrderSortInput { CreatedAt = sortDirection }
        };

        return new[] { sortInput };
    }

    private async Task NextPageAsync()
    {
        if (!hasNextPage) return;

        if (startCursor != null)
        {
            cursorHistory.Push(startCursor);
        }

        currentCursor = endCursor;
        currentPageNumber++;
        await LoadOrdersAsync();
    }

    private async Task PreviousPageAsync()
    {
        if (!hasPreviousPage || cursorHistory.Count == 0) return;

        currentCursor = cursorHistory.Pop();
        currentPageNumber--;
        await LoadOrdersAsync();
    }

    private async Task FirstPageAsync()
    {
        currentCursor = null;
        cursorHistory.Clear();
        currentPageNumber = 1;
        await LoadOrdersAsync();
    }

    private async Task LastPageAsync()
    {
        // For cursor-based pagination, jump to last by continuously loading next pages
        while (hasNextPage)
        {
            if (startCursor != null)
            {
                cursorHistory.Push(startCursor);
            }
            currentCursor = endCursor;
            currentPageNumber++;
            await LoadOrdersAsync();
        }
    }

    private async Task ApplyFiltersAsync()
    {
        currentCursor = null;
        cursorHistory.Clear();
        currentPageNumber = 1;
        await LoadOrdersAsync();
    }

    private async Task ClearFiltersAsync()
    {
        filterStatus = null;
        searchText = string.Empty;
        await ApplyFiltersAsync();
    }

    private async Task SortByAsync(string field)
    {
        if (sortField == field)
        {
            sortDescending = !sortDescending;
        }
        else
        {
            sortField = field;
            sortDescending = true;
        }

        currentCursor = null;
        cursorHistory.Clear();
        currentPageNumber = 1;
        await LoadOrdersAsync();
    }

    private void SetupSubscriptions()
    {
        try
        {
            // Subscribe to order created events
            _createSubscription = GraphQLClient.OnOrderCreated
                .Watch()
                .Subscribe(result =>
                {
                    if (result.Data?.OnOrderCreated != null)
                    {
                        InvokeAsync(() =>
                        {
                            var newOrder = Mapper.Map<OrderViewModel>(result.Data.OnOrderCreated);

                            // Add new order to the beginning of the list if not already present
                            if (!orders.Any(o => o.Id == newOrder.Id))
                            {
                                orders.Insert(0, newOrder);
                                StateHasChanged();
                            }
                        });
                    }
                });

            // Subscribe to order updated events
            _updateSubscription = GraphQLClient.OnOrderUpdated
                .Watch()
                .Subscribe(result =>
                {
                    if (result.Data?.OnOrderUpdated != null)
                    {
                        InvokeAsync(() =>
                        {
                            var updatedOrder = Mapper.Map<OrderViewModel>(result.Data.OnOrderUpdated);

                            // Find and update the existing order in the list
                            var existingOrder = orders.FirstOrDefault(o => o.Id == updatedOrder.Id);
                            if (existingOrder != null)
                            {
                                var index = orders.IndexOf(existingOrder);
                                orders[index] = updatedOrder;
                                StateHasChanged();
                            }
                        });
                    }
                });

            // Subscribe to order deleted events
            _deleteSubscription = GraphQLClient.OnOrderDeleted
                .Watch()
                .Subscribe(result =>
                {
                    if (result.Data?.OnOrderDeleted != null)
                    {
                        InvokeAsync(() =>
                        {
                            var deletedOrderId = result.Data.OnOrderDeleted;

                            // Remove order from list
                            var orderToRemove = orders.FirstOrDefault(o => o.Id == deletedOrderId);
                            if (orderToRemove != null)
                            {
                                orders.Remove(orderToRemove);
                                StateHasChanged();
                            }
                        });
                    }
                });

            subscriptionsActive = true;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[Subscription] Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Subscription] Failed to setup realtime: {ex.Message}");
        }
    }

    private void ShowDeleteConfirmation(Guid orderId, string orderNumber, string customerName)
    {
        deleteOrderId = orderId;
        deleteOrderNumber = orderNumber;
        deleteCustomerName = customerName;
        showDeleteConfirmation = true;
    }

    private void CancelDelete()
    {
        showDeleteConfirmation = false;
        deleteOrderId = Guid.Empty;
        deleteOrderNumber = string.Empty;
        deleteCustomerName = string.Empty;
    }

    private async Task ConfirmDelete()
    {
        try
        {
            isDeleting = true;
            errorMessage = null;

            var result = await GraphQLClient.DeleteOrder.ExecuteAsync(deleteOrderId);

            if (result.Data?.DeleteOrder == true)
            {
                // Remove order from list
                var orderToRemove = orders.FirstOrDefault(o => o.Id == deleteOrderId);
                if (orderToRemove != null)
                {
                    orders.Remove(orderToRemove);
                }

                showDeleteConfirmation = false;
                CancelDelete();
                StateHasChanged();
            }
            else if (result.Errors?.Count > 0)
            {
                // GraphQL standard - errors in top-level errors array
                errorMessage = ErrorMessageHelper.GetErrorMessage(result);
                showDeleteConfirmation = false;
            }
            else
            {
                errorMessage = OrderListResources.FailedToDeleteOrderGeneric;
                showDeleteConfirmation = false;
            }
        }
        catch (HttpRequestException ex)
        {
            errorMessage = ErrorMessageHelper.HandleException(ex, "DeleteOrder");
            showDeleteConfirmation = false;
        }
        catch (OperationCanceledException ex)
        {
            errorMessage = ErrorMessageHelper.HandleException(ex, "DeleteOrder");
            showDeleteConfirmation = false;
        }
        catch (Exception ex)
        {
            errorMessage = ErrorMessageHelper.HandleException(ex, "DeleteOrder");
            showDeleteConfirmation = false;
        }
        finally
        {
            isDeleting = false;
        }
    }

    private string GetStatusBadgeClass(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => "bg-warning text-dark",
            OrderStatus.Processing => "bg-info",
            OrderStatus.Completed => "bg-success",
            OrderStatus.Cancelled => "bg-danger",
            _ => "bg-secondary"
        };
    }

    public void Dispose()
    {
        _createSubscription?.Dispose();
        _updateSubscription?.Dispose();
        _deleteSubscription?.Dispose();
    }
}
