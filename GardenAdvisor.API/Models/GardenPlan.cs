namespace GardenAdvisor.API.Models;

public class GardenPlan
{
    public string Id { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public GardenRequest Request { get; set; } = new();
    public GardenDesignResult Result { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
