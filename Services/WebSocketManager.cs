using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class WebSocketManager
{
    private readonly ConcurrentBag<WebSocket> _sockets = new ConcurrentBag<WebSocket>();

    public void AddSocket(WebSocket socket)
    {
        _sockets.Add(socket);
    }

    public void RemoveSocket(WebSocket socket)
    {
        // Remove the socket (ConcurrentBag doesn't have a direct remove, so we rely on cleanup in BroadcastAsync)
    }

    public async Task BroadcastAsync(string message, CancellationToken cancellationToken)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        var segment = new ArraySegment<byte>(buffer);

        foreach (var socket in _sockets)
        {
            if (socket.State == WebSocketState.Open)
            {
                try
                {
                    await socket.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);
                }
                catch
                {
                    // Handle or log errors (e.g., socket closed unexpectedly)
                }
            }
        }

        // Clean up closed sockets
        var closedSockets = _sockets.Where(s => s.State != WebSocketState.Open).ToList();
        foreach (var socket in closedSockets)
        {
            _sockets.TryTake(out _);
        }
    }
}