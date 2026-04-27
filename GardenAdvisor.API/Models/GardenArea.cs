namespace GardenAdvisor.API.Models;

public class GardenArea
{
    public List<LatLng> Polygon { get; set; } = new();
    public double AreaSquareMeters { get; set; }
}

public class LatLng
{
    public double Lat { get; set; }
    public double Lng { get; set; }
}
