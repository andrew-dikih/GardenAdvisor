namespace GardenAdvisor.API.Services;
using GardenAdvisor.API.Models;

public interface IClimateZoneService
{
    ClimateZone GetClimateZone(double latitude, double longitude);
}
