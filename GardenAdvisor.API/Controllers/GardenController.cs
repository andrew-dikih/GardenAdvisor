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
    private readonly IGardenPlanRepository? _gardenPlanRepository;
    private readonly ILogger<GardenController> _logger;

    public GardenController(
        IGardenDesignService gardenDesignService,
        IEmailService emailService,
        IHubContext<GardenProcessingHub> hubContext,
        ILogger<GardenController> logger,
        IGardenPlanRepository? gardenPlanRepository = null)
    {
        _gardenDesignService = gardenDesignService;
        _emailService = emailService;
        _hubContext = hubContext;
        _logger = logger;
        _gardenPlanRepository = gardenPlanRepository;
    }

    [HttpPost("design")]
    public IActionResult StartGardenDesign([FromBody] GardenRequest request)
    {
        if (string.IsNullOrEmpty(request.SessionId))
            request.SessionId = Guid.NewGuid().ToString();

        // Start background processing and track it (but don't await in response)
        _ = ProcessGardenDesignAsync(request);

        return Accepted(new { sessionId = request.SessionId, message = "Garden design started" });
    }

    [HttpGet("plans/{sessionId}")]
    public async Task<IActionResult> GetPlanBySessionId(string sessionId, CancellationToken cancellationToken)
    {
        if (_gardenPlanRepository is null)
            return StatusCode(503, new { message = "Plan storage is not configured." });

        var plan = await _gardenPlanRepository.GetPlanBySessionIdAsync(sessionId, cancellationToken);
        if (plan is null)
            return NotFound();

        return Ok(plan);
    }

    [HttpGet("plans/user/{userId}")]
    public async Task<IActionResult> GetPlansByUserId(string userId, CancellationToken cancellationToken)
    {
        if (_gardenPlanRepository is null)
            return StatusCode(503, new { message = "Plan storage is not configured." });

        var plans = await _gardenPlanRepository.GetPlansByUserIdAsync(userId, cancellationToken);
        return Ok(plans);
    }

    /// <summary>
    /// Background task for processing garden design with real-time SignalR updates.
    /// Exceptions are logged but not thrown, as this runs after the HTTP response is sent.
    /// </summary>
    private async Task ProcessGardenDesignAsync(GardenRequest request)
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

            if (_gardenPlanRepository is not null)
            {
                var plan = new GardenPlan
                {
                    Id = result.SessionId,
                    SessionId = result.SessionId,
                    UserId = request.UserId,
                    Request = request,
                    Result = result,
                    GeneratedAt = result.GeneratedAt
                };
                await _gardenPlanRepository.SavePlanAsync(plan);
            }

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
    }
}
