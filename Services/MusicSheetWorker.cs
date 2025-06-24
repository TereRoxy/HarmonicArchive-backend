using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using System.Net.WebSockets;
using HarmonicArchiveBackend.Services;

public class MusicSheetWorker : IHostedService, IDisposable
{
    private static bool _isRunning = false;
    private static CancellationTokenSource _cts = new CancellationTokenSource();
    private readonly IServiceProvider _serviceProvider;
    private readonly WebSocketManager _webSocketManager;
    private Task _workerTask;

    public MusicSheetWorker(IServiceProvider serviceProvider, WebSocketManager webSocketManager)
    {
        _serviceProvider = serviceProvider;
        _webSocketManager = webSocketManager;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Start the worker if it's not already running
        if (!_isRunning)
        {
            _isRunning = true;
            _cts = new CancellationTokenSource();
            _workerTask = Task.Run(() => RunWorkerAsync(_cts.Token));
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Stop the worker
        if (_isRunning)
        {
            _isRunning = false;
            _cts.Cancel();
        }
        return Task.CompletedTask;
    }

    private async Task RunWorkerAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Use service scope to resolve MusicSheetService
                using (var scope = _serviceProvider.CreateScope())
                {
                    var musicSheetService = scope.ServiceProvider.GetRequiredService<MusicSheetService>();

                    // Generate one new music sheet using MusicSheetGenerator
                    var newMusicSheets = MusicSheetGenerator.GenerateMusicSheets(1);
                    var newMusicSheet = newMusicSheets[0];

                    // Set UserId to a default or system value (e.g., 0 for generated sheets)
                    newMusicSheet.UserId = 0;

                    // Save the generated music sheet to the database
                    await musicSheetService.AddMusicSheetFromDtoAsync(newMusicSheet);

                    // Serialize the music sheet to JSON
                    var message = JsonSerializer.Serialize(newMusicSheet);

                    // Broadcast to all connected WebSocket clients
                    await _webSocketManager.BroadcastAsync(message, cancellationToken);
                }

                // Wait for a specified interval (e.g., 30 seconds) before generating the next sheet
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Exit the loop if cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                // Log the error (use proper logging in production)
                Console.WriteLine($"Worker error: {ex.Message}");
            }
        }
    }

    public static void Start()
    {
        // This method is called by the controller
        if (!_isRunning)
        {
            _isRunning = true;
            _cts = new CancellationTokenSource();
            // The actual task is started in StartAsync
        }
    }

    public static void Stop()
    {
        // This method is called by the controller
        if (_isRunning)
        {
            _isRunning = false;
            _cts.Cancel();
        }
    }

    public void Dispose()
    {
        _cts?.Dispose();
    }
}