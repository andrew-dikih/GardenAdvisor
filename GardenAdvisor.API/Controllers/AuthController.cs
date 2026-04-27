using Microsoft.AspNetCore.Mvc;
using GardenAdvisor.API.Models;

namespace GardenAdvisor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    [HttpPost("register-email")]
    public IActionResult RegisterEmail([FromBody] EmailRegistrationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("Email is required");

        _logger.LogInformation("Email registration: {Email}", request.Email);

        var session = new UserSession
        {
            Email = request.Email,
            Name = request.Name ?? request.Email
        };

        return Ok(new { sessionId = session.SessionId, email = session.Email, name = session.Name });
    }

    [HttpGet("google")]
    public IActionResult GoogleLogin([FromQuery] string returnUrl = "/")
    {
        return Ok(new
        {
            message = "Google OAuth would be configured with actual credentials in production.",
            authUrl = "https://accounts.google.com/o/oauth2/auth"
        });
    }

    [HttpGet("google/callback")]
    public IActionResult GoogleCallback([FromQuery] string code, [FromQuery] string state)
    {
        var session = new UserSession
        {
            OAuthProvider = "google",
            Name = "Google User"
        };
        return Ok(new { sessionId = session.SessionId });
    }
}

public class EmailRegistrationRequest
{
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
}
