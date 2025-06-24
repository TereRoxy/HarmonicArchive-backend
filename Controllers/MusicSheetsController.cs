using HarmonicArchiveBackend.Data;
using HarmonicArchiveBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

[ApiController]
[Route("api/[controller]")]
public class MusicSheetsController : ControllerBase
{
    private readonly MusicSheetService _service;
    private readonly WebSocketManager _worker;

    public MusicSheetsController(MusicSheetService service, WebSocketManager worker)
    {
        _service = service;
        _worker = worker;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetMusicSheets(
        [FromQuery] string? title,
        [FromQuery] string? composer,
        [FromQuery] string? genres,
        [FromQuery] string? instruments,
        [FromQuery] string? _sort = "title",
        [FromQuery] string? _order = "asc",
        [FromQuery] int _page = 1,
        [FromQuery] int _limit = 10)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();

        var userId = int.Parse(userIdClaim.Value);

        if (_page < 1)
            return BadRequest(new { Message = "Page number must be greater than 0." });

        if (_limit < 1)
            return BadRequest(new { Message = "Limit must be greater than 0." });


        var genreList = genres?.Split(',').Select(g => g.Trim()).ToList();
        var instrumentList = instruments?.Split(',').Select(i => i.Trim()).ToList();

        var (musicSheets, totalCount) = await _service.GetAllMusicSheetsAsync(
            title, composer, genreList, instrumentList, _sort, _order, _page, _limit, userId);

        return Ok(new
        {
            Data = musicSheets,
            TotalCount = totalCount,
        });
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetMusicSheetById(int id)
    {
        var musicSheet = await _service.GetMusicSheetByIdAsync(id);
        if (musicSheet == null)
        {
            return NotFound(new { Message = "Music sheet not found." });
        }
        return Ok(musicSheet);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateMusicSheet([FromBody] MusicSheetDto musicSheet)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();

        var userId = int.Parse(userIdClaim.Value);

        // Ensure the userId from the payload matches the authenticated user's ID
        if (musicSheet.UserId != userId)
            return Unauthorized("User ID mismatch.");

        await _service.AddMusicSheetFromDtoAsync(musicSheet);
        return Ok(new { Message = "Music Sheet created successfully" });
    }

    [HttpPatch("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateMusicSheet(int id, [FromBody] MusicSheetDto musicSheetDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _service.UpdateMusicSheetFromDtoAsync(id, musicSheetDto);
            return Ok(new { Message = "Music sheet updated successfully." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteMusicSheet(int id)
    {
        await _service.DeleteMusicSheetAsync(id);
        return NoContent();
    }

    [HttpPost("toggle-worker")]
    public IActionResult ToggleWorker([FromQuery] bool isRunning)
    {
        if (isRunning)
        {
            MusicSheetWorker.Start();
        }
        else
        {
            MusicSheetWorker.Stop();
        }
        return Ok(new { Message = $"Worker is now {(isRunning ? "running" : "stopped")}." });
    }

    [HttpGet("ws")]
    public async Task GetWebSocket()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            _worker.AddSocket(webSocket); // Add the WebSocket to the worker's WebSocketManager

            var buffer = new byte[1024 * 4];
            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    // Keep the connection alive by receiving messages
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    // If the client closes the connection, break the loop
                    if (result.CloseStatus.HasValue)
                    {
                        break;
                    }
                }
            }
            finally
            {
                _worker.RemoveSocket(webSocket); // Remove the WebSocket when the connection is closed
            }
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    [HttpPost("upload")]
    [Authorize]
    public async Task<IActionResult> UploadMusicFile([FromForm] MusicFileUploadDto dto)
    {
        var musicFile = dto.MusicFile;
        if (musicFile == null || musicFile.Length == 0)
            return BadRequest("No file uploaded.");

        // Validate file type (only PDFs)
        if (!musicFile.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only PDF files are allowed.");

        var uploadsFolder = Path.Combine("UploadedFiles", "Music");
        var fullUploadsPath = Path.GetFullPath(uploadsFolder);

        if (!Directory.Exists(fullUploadsPath))
        {
            Directory.CreateDirectory(fullUploadsPath);
        }

        // Sanitize filename to prevent path traversal
        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(musicFile.FileName); // Use GUID for uniqueness
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await musicFile.CopyToAsync(stream);
        }

        return Ok(new { filePath = $"/UploadedFiles/Music/{fileName}" });
    }

    [HttpGet("current/tags")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUserTags()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();

        var userId = int.Parse(userIdClaim.Value);

        var (genres, instruments) = await _service.GetUniqueTagsForUserAsync(userId);

        return Ok(new
        {
            Genres = genres,
            Instruments = instruments
        });
    }

}
