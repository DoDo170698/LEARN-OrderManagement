using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace OrderManagement.Blazor.Services;

/// <summary>
/// Service for handling GraphQL subscriptions via WebSocket
/// </summary>
public class OrderSubscriptionService : IAsyncDisposable
{
    private ClientWebSocket? _webSocket;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly string _webSocketUrl;

    public event Func<Order, Task>? OnOrderCreated;
    public event Func<Order, Task>? OnOrderUpdated;

    public OrderSubscriptionService(IConfiguration configuration)
    {
        _webSocketUrl = configuration["GraphQL:WebSocketEndpoint"]!;
    }

    public async Task ConnectAsync()
    {
        try
        {
            _webSocket = new ClientWebSocket();
            _cancellationTokenSource = new CancellationTokenSource();

            await _webSocket.ConnectAsync(new Uri(_webSocketUrl), _cancellationTokenSource.Token);

            // Send connection init message
            var initMessage = new
            {
                type = "connection_init",
                payload = new { }
            };

            await SendMessageAsync(JsonSerializer.Serialize(initMessage));

            // Start listening for messages
            _ = Task.Run(() => ListenForMessagesAsync(_cancellationTokenSource.Token));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WebSocket connection error: {ex.Message}");
        }
    }

    public async Task SubscribeToOrderCreatedAsync()
    {
        if (_webSocket?.State != WebSocketState.Open)
        {
            await ConnectAsync();
        }

        var subscription = new
        {
            id = "1",
            type = "subscribe",
            payload = new
            {
                query = @"
                    subscription {
                        onOrderCreated {
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
                    }"
            }
        };

        await SendMessageAsync(JsonSerializer.Serialize(subscription));
    }

    public async Task SubscribeToOrderUpdatedAsync()
    {
        if (_webSocket?.State != WebSocketState.Open)
        {
            await ConnectAsync();
        }

        var subscription = new
        {
            id = "2",
            type = "subscribe",
            payload = new
            {
                query = @"
                    subscription {
                        onOrderUpdated {
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
                    }"
            }
        };

        await SendMessageAsync(JsonSerializer.Serialize(subscription));
    }

    private async Task SendMessageAsync(string message)
    {
        if (_webSocket?.State == WebSocketState.Open)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    private async Task ListenForMessagesAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];

        try
        {
            while (_webSocket?.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationToken);
                    break;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                await ProcessMessageAsync(message);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WebSocket listening error: {ex.Message}");
        }
    }

    private async Task ProcessMessageAsync(string message)
    {
        try
        {
            using var doc = JsonDocument.Parse(message);
            var root = doc.RootElement;

            if (!root.TryGetProperty("type", out var typeElement))
                return;

            var type = typeElement.GetString();

            if (type == "next" && root.TryGetProperty("payload", out var payload))
            {
                if (payload.TryGetProperty("data", out var data))
                {
                    if (data.TryGetProperty("onOrderCreated", out var orderCreated))
                    {
                        var order = JsonSerializer.Deserialize<Order>(orderCreated.GetRawText(), new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (order != null && OnOrderCreated != null)
                        {
                            await OnOrderCreated.Invoke(order);
                        }
                    }
                    else if (data.TryGetProperty("onOrderUpdated", out var orderUpdated))
                    {
                        var order = JsonSerializer.Deserialize<Order>(orderUpdated.GetRawText(), new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (order != null && OnOrderUpdated != null)
                        {
                            await OnOrderUpdated.Invoke(order);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Message processing error: {ex.Message}");
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            // Cancel the token first if not already disposed
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }
        }
        catch (ObjectDisposedException)
        {
            // Token source already disposed, ignore
        }

        try
        {
            // Close WebSocket gracefully if still open
            if (_webSocket?.State == WebSocketState.Open)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WebSocket close error: {ex.Message}");
        }

        // Dispose resources
        _webSocket?.Dispose();
        _cancellationTokenSource?.Dispose();
    }
}
