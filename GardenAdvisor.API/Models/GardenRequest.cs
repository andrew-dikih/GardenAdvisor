namespace GardenAdvisor.API.Models;

public class GardenRequest
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public Address Address { get; set; } = new();
    public GardenArea GardenArea { get; set; } = new();
    public List<string> SelectedPlantIds { get; set; } = new();
    public string? UserEmail { get; set; }
    public string? UserId { get; set; }
}
