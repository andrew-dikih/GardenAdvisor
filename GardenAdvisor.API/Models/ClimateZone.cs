namespace GardenAdvisor.API.Models;

public class ClimateZone
{
    public string ZoneCode { get; set; } = string.Empty;
    public string ZoneName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double MinTempF { get; set; }
    public double MaxTempF { get; set; }
    public string KoppenClassification { get; set; } = string.Empty;
    public int AverageFrostFreeDays { get; set; }
    public string LastFrostMonth { get; set; } = string.Empty;
    public string FirstFrostMonth { get; set; } = string.Empty;
}
