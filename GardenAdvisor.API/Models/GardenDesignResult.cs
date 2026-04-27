namespace GardenAdvisor.API.Models;

public class GardenDesignResult
{
    public string SessionId { get; set; } = string.Empty;
    public ClimateZone ClimateZone { get; set; } = new();
    public List<PlantingScheduleItem> Schedule { get; set; } = new();
    public List<Plant> AdditionalRecommendations { get; set; } = new();
    public List<PlantWarning> Warnings { get; set; } = new();
    public string LayoutImageUrl { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}

public class PlantWarning
{
    public Plant Plant { get; set; } = new();
    public string Reason { get; set; } = string.Empty;
}
