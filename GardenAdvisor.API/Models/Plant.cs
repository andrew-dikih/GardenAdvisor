namespace GardenAdvisor.API.Models;

public class Plant
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public List<string> GrowingZones { get; set; } = new();
    public int DaysToMaturity { get; set; }
    public string SunRequirement { get; set; } = string.Empty;
    public string WaterRequirement { get; set; } = string.Empty;
    public double SpacingMeters { get; set; }
}
