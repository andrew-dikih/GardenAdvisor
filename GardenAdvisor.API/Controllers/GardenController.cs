using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using GardenAdvisor.API.Models;
using GardenAdvisor.API.Services;
using GardenAdvisor.API.Hubs;

namespace GardenAdvisor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GardenController : ControllerBase
{
    private readonly IGardenDesignService _gardenDesignService;
    private readonly IEmailService _emailService;
    private readonly IHubContext<GardenProcessingHub> _hubContext;
    private readonly ILogger<GardenController> _logger;

    public GardenController(
        IGardenDesignService gardenDesignService,
        IEmailService emailService,
        IHubContext<GardenProcessingHub> hubContext,
        ILogger<GardenController> logger)
    {
        _gardenDesignService = gardenDesignService;
        _emailService = emailService;
        _hubContext = hubContext;
        _logger = logger;
    }

    [HttpPost("design")]
    public IActionResult StartGardenDesign([FromBody] GardenRequest request)
    {
        if (string.IsNullOrEmpty(request.SessionId))
            request.SessionId = Guid.NewGuid().ToString();

        _ = Task.Run(async () =>
        {
            try
            {
                await _hubContext.Clients.Group(request.SessionId)
                    .SendAsync("ProcessingUpdate", new { status = "started", message = "Starting garden design..." });

                var progress = new Progress<string>(async message =>
                {
                    await _hubContext.Clients.Group(request.SessionId)
                        .SendAsync("ProcessingUpdate", new { status = "processing", message });
                });

                var result = await _gardenDesignService.DesignGardenAsync(request, progress);

                await _hubContext.Clients.Group(request.SessionId)
                    .SendAsync("ProcessingComplete", result);

                if (!string.IsNullOrEmpty(request.UserEmail))
                {
                    await _emailService.SendScheduleEmailAsync(request.UserEmail, request.UserEmail, result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing garden design for session {SessionId}", request.SessionId);
                await _hubContext.Clients.Group(request.SessionId)
                    .SendAsync("ProcessingError", new { message = "An error occurred processing your garden design." });
            }
        });

        return Accepted(new { sessionId = request.SessionId, message = "Garden design started" });
    }
}
