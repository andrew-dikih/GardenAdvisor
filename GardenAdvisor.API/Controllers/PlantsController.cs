using Microsoft.AspNetCore.Mvc;
using GardenAdvisor.API.Services;

namespace GardenAdvisor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlantsController : ControllerBase
{
    private readonly IPlantService _plantService;

    public PlantsController(IPlantService plantService)
    {
        _plantService = plantService;
    }

    [HttpGet("categories")]
    public IActionResult GetCategories()
    {
        return Ok(_plantService.GetCategories());
    }

    [HttpGet("category/{categoryId}")]
    public IActionResult GetPlantsByCategory(string categoryId)
    {
        return Ok(_plantService.GetPlantsByCategory(categoryId));
    }

    [HttpGet("{id}")]
    public IActionResult GetPlant(string id)
    {
        var plant = _plantService.GetPlantById(id);
        if (plant == null) return NotFound();
        return Ok(plant);
    }
}
