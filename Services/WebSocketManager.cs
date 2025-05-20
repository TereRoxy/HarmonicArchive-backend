using System.Net.WebSockets;
using System.Text;

namespace HarmonicArchiveBackend.Services;
public class WebSocketManager
{
    private readonly List<WebSocket> _sockets = new();

    public void AddSocket(WebSocket socket)
    {
        _sockets.Add(socket);
    }

    public async Task BroadcastMessageAsync(string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        var tasks = _sockets
            .Where(s => s.State == WebSocketState.Open)
            .Select(async socket =>
            {
                try
                {
                    await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch
                {
                    // Handle errors (e.g., remove closed sockets)
                    _sockets.Remove(socket);
                }
            });

        await Task.WhenAll(tasks);
    }

    public void RemoveSocket(WebSocket socket)
    {
        _sockets.Remove(socket);
    }
}
