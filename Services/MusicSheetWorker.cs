using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HarmonicArchiveBackend.Services;
using WebSocketManager = HarmonicArchiveBackend.Services.WebSocketManager;

public class MusicSheetWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MusicSheetWorker> _logger;
    private readonly WebSocketManager _webSocketManager;
    private bool _isRunning = false;

    public MusicSheetWorker(IServiceProvider serviceProvider, ILogger<MusicSheetWorker> logger, WebSocketManager webSocketManager)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _webSocketManager = webSocketManager;
    }

    public void ToggleWorker(bool isRunning)
    {
        _isRunning = isRunning;
        _logger.LogInformation($"MusicSheetWorker is now {(isRunning ? "running" : "stopped")}.");
    }

    // MusicSheetWorker.cs
    public void AddWebSocket(WebSocket webSocket)
    {
        _webSocketManager.AddSocket(webSocket);
    }

    public void RemoveWebSocket(WebSocket webSocket)
    {
        _webSocketManager.RemoveSocket(webSocket);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_isRunning)
            {
                try
                {
                    var fakeMusicSheet = MusicSheetGenerator.GenerateMusicSheets(1).First();
                    // Add the generated music sheet to the database
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var musicSheetService = scope.ServiceProvider.GetRequiredService<MusicSheetService>();
                        await musicSheetService.AddMusicSheetFromDtoAsync(fakeMusicSheet);
                    }

                    var message = JsonSerializer.Serialize(fakeMusicSheet);
                    await _webSocketManager.BroadcastMessageAsync(message);
                    _logger.LogInformation("Sent new music sheet via WebSocket");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in worker execution: {ex.Message}");
                }
            }
            await Task.Delay(5000, stoppingToken);
        }
    }
}
