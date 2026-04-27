namespace GardenAdvisor.API.Models;

public class PlantingScheduleItem
{
    public Plant Plant { get; set; } = new();
    public string StartIndoorsMonth { get; set; } = string.Empty;
    public string TransplantMonth { get; set; } = string.Empty;
    public string DirectSowMonth { get; set; } = string.Empty;
    public string HarvestStartMonth { get; set; } = string.Empty;
    public string HarvestEndMonth { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public LatLng? RecommendedPlotPosition { get; set; }
}
