namespace GardenAdvisor.API.Services;
using GardenAdvisor.API.Models;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendScheduleEmailAsync(string email, string userName, GardenDesignResult result)
    {
        // MVP: log to console. Production would use SMTP or SendGrid.
        _logger.LogInformation(
            "📧 [EMAIL] To: {Email} | Subject: Your GardenAdvisor Planting Schedule | Zone: {Zone} | Plants: {Count} | Session: {Session}",
            email,
            result.ClimateZone.ZoneName,
            result.Schedule.Count,
            result.SessionId);

        _logger.LogInformation("Email body preview: Hi {Name}, your garden design for {Zone} is ready with {Count} plants scheduled.",
            userName, result.ClimateZone.ZoneName, result.Schedule.Count);

        return Task.CompletedTask;
    }
}
