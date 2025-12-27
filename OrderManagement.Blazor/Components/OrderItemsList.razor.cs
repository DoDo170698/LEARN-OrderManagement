using AutoMapper;
using Microsoft.AspNetCore.Components;
using OrderManagement.Blazor.GraphQL;
using OrderManagement.Blazor.ViewModels;

namespace OrderManagement.Blazor.Components;

/// <summary>
/// Component for displaying order items with paging, filtering, and sorting
/// </summary>
public partial class OrderItemsList
{
    [Inject] private IOrderManagementClient GraphQLClient { get; set; } = default!;
    [Inject] private IMapper Mapper { get; set; } = default!;

    [Parameter, EditorRequired]
    public Guid OrderId { get; set; }

    private List<OrderItemViewModel> items = new();
    private bool isLoading = true;
    private string? errorMessage;

    // Paging state
    private int pageSize = 10;
    private string? currentCursor = null;
    private string? startCursor = null;
    private string? endCursor = null;
    private bool hasNextPage = false;
    private bool hasPreviousPage = false;
    private Stack<string> cursorHistory = new();
    private int currentPageNumber = 1;

    // Filter state
    private string searchText = string.Empty;

    // Sort state
    private string sortField = "PRODUCT_NAME";
    private bool sortDescending = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadItemsAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (OrderId != Guid.Empty)
        {
            await LoadItemsAsync();
        }
    }

    private async Task LoadItemsAsync()
    {
        try
        {
            isLoading = true;
            errorMessage = null;

            // Build filter
            var whereFilter = BuildFilter();

            // Build sort
            var orderSort = BuildSort();

            // Load items with paging, filtering, and sorting
            var result = await GraphQLClient.GetOrderItemsList.ExecuteAsync(
                orderId: OrderId,
                first: pageSize,
                after: currentCursor,
                last: null,
                before: null,
                where: whereFilter,
                order: orderSort,
                CancellationToken.None
            );

            if (result.Data?.OrderItems != null)
            {
                items = result.Data.OrderItems.Edges?
                    .Select(edge => Mapper.Map<OrderItemViewModel>(edge.Node))
                    .ToList() ?? new List<OrderItemViewModel>();

                // Update pagination state
                hasNextPage = result.Data.OrderItems.PageInfo?.HasNextPage ?? false;
                hasPreviousPage = result.Data.OrderItems.PageInfo?.HasPreviousPage ?? false;
                startCursor = result.Data.OrderItems.PageInfo?.StartCursor;
                endCursor = result.Data.OrderItems.PageInfo?.EndCursor;
            }
            else if (result.Errors?.Count > 0)
            {
                errorMessage = string.Join(", ", result.Errors.Select(e => e.Message));
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to load items: {ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }

    private OrderItemFilterInput? BuildFilter()
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return null;

        return new OrderItemFilterInput
        {
            ProductName = new StringOperationFilterInput
            {
                Contains = searchText
            }
        };
    }

    private IReadOnlyList<OrderItemSortInput>? BuildSort()
    {
        var sortInput = new OrderItemSortInput();
        var sortDirection = sortDescending ? SortEnumType.Desc : SortEnumType.Asc;

        switch (sortField)
        {
            case "PRODUCT_NAME":
            default:
                sortInput.ProductName = sortDirection;
                break;
            case "QUANTITY":
                sortInput.Quantity = sortDirection;
                break;
            case "UNIT_PRICE":
                sortInput.UnitPrice = sortDirection;
                break;
            case "SUBTOTAL":
                sortInput.Subtotal = sortDirection;
                break;
        }

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
        await LoadItemsAsync();
    }

    private async Task PreviousPageAsync()
    {
        if (!hasPreviousPage || cursorHistory.Count == 0) return;

        currentCursor = cursorHistory.Pop();
        currentPageNumber--;
        await LoadItemsAsync();
    }

    private async Task FirstPageAsync()
    {
        currentCursor = null;
        cursorHistory.Clear();
        currentPageNumber = 1;
        await LoadItemsAsync();
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
            await LoadItemsAsync();
        }
    }

    private async Task ApplyFiltersAsync()
    {
        currentCursor = null;
        cursorHistory.Clear();
        currentPageNumber = 1;
        await LoadItemsAsync();
    }

    private async Task ClearFiltersAsync()
    {
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
            sortDescending = false;
        }

        currentCursor = null;
        cursorHistory.Clear();
        await LoadItemsAsync();
    }
}
