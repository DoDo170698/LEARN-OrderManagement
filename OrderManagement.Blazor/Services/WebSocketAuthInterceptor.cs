using StrawberryShake.Transport.WebSockets;

namespace OrderManagement.Blazor.Services;

/// <summary>
/// Interceptor to add authentication token to WebSocket connections
/// </summary>
public class WebSocketAuthInterceptor : ISocketConnectionInterceptor
{
    private readonly string _token;

    public WebSocketAuthInterceptor(string token)
    {
        _token = token;
    }

    public ValueTask<object?> CreateConnectionInitPayload(
        ISocketProtocol protocol,
        CancellationToken cancellationToken)
    {
        var payload = new Dictionary<string, object>
        {
            ["Authorization"] = $"Bearer {_token}"
        };

        return new ValueTask<object?>(payload);
    }

    public void OnConnectionOpened(ISocketClient client)
    {
        // No action needed
    }

    public void OnConnectionClosed(ISocketClient client)
    {
        // No action needed
    }
}
