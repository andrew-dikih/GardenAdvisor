namespace GardenAdvisor.API.Models;

public class UserSession
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? OAuthProvider { get; set; }
    public string? OAuthSubject { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Address? Address { get; set; }
    public GardenArea? GardenArea { get; set; }
    public List<string> SelectedPlantIds { get; set; } = new();
    public GardenDesignResult? LastDesignResult { get; set; }
}
