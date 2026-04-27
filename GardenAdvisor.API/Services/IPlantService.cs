namespace GardenAdvisor.API.Services;
using GardenAdvisor.API.Models;

public interface IPlantService
{
    List<PlantCategory> GetCategories();
    List<Plant> GetPlantsByCategory(string categoryId);
    Plant? GetPlantById(string id);
    List<Plant> GetPlantsByIds(List<string> ids);
}
