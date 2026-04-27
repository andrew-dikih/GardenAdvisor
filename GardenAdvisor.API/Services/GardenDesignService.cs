namespace GardenAdvisor.API.Services;
using GardenAdvisor.API.Models;

public class GardenDesignService : IGardenDesignService
{
    private readonly IClimateZoneService _climateZoneService;
    private readonly IPlantService _plantService;

    public GardenDesignService(IClimateZoneService climateZoneService, IPlantService plantService)
    {
        _climateZoneService = climateZoneService;
        _plantService = plantService;
    }

    public async Task<GardenDesignResult> DesignGardenAsync(GardenRequest request, IProgress<string> progress, CancellationToken cancellationToken = default)
    {
        progress.Report("Analyzing location coordinates...");
        await Task.Delay(800, cancellationToken);

        var climateZone = _climateZoneService.GetClimateZone(request.Address.Latitude, request.Address.Longitude);

        progress.Report($"Climate zone identified: {climateZone.ZoneName}");
        await Task.Delay(600, cancellationToken);

        progress.Report("Evaluating selected plants for your climate zone...");
        await Task.Delay(700, cancellationToken);

        var selectedPlants = _plantService.GetPlantsByIds(request.SelectedPlantIds);
        var suitablePlants = new List<Plant>();
        var warnings = new List<PlantWarning>();

        foreach (var plant in selectedPlants)
        {
            if (plant.GrowingZones.Contains(climateZone.ZoneCode))
                suitablePlants.Add(plant);
            else
                warnings.Add(new PlantWarning
                {
                    Plant = plant,
                    Reason = $"{plant.Name} grows best in zones {string.Join(", ", plant.GrowingZones)}, but your zone is {climateZone.ZoneCode}."
                });
        }

        progress.Report("Building planting schedule...");
        await Task.Delay(900, cancellationToken);

        var schedule = BuildSchedule(suitablePlants, climateZone, request.GardenArea);

        progress.Report("Generating plant layout for your garden area...");
        await Task.Delay(700, cancellationToken);

        progress.Report("Finding additional plant recommendations...");
        await Task.Delay(600, cancellationToken);

        var allPlants = _plantService.GetPlantsByIds(
            new[] { "vegetables", "fruits", "flowers" }
                .SelectMany(c => _plantService.GetPlantsByCategory(c))
                .Select(p => p.Id).ToList());

        var eligiblePlants = allPlants
            .Where(p => p.GrowingZones.Contains(climateZone.ZoneCode)
                     && !request.SelectedPlantIds.Contains(p.Id))
            .ToList();

        // Shuffle and select recommendations
        var recommendations = ShuffleAndTake(eligiblePlants, 5);

        progress.Report("Finalizing your garden design...");
        await Task.Delay(500, cancellationToken);

        return new GardenDesignResult
        {
            SessionId = request.SessionId,
            ClimateZone = climateZone,
            Schedule = schedule,
            AdditionalRecommendations = recommendations,
            Warnings = warnings,
            GeneratedAt = DateTime.UtcNow
        };
    }

    private List<PlantingScheduleItem> BuildSchedule(List<Plant> plants, ClimateZone zone, GardenArea gardenArea)
    {
        var schedule = new List<PlantingScheduleItem>();
        var months = new[] { "January","February","March","April","May","June","July","August","September","October","November","December" };
        var lastFrostIdx = Array.IndexOf(months, zone.LastFrostMonth);
        if (lastFrostIdx < 0) lastFrostIdx = 3; // default April

        var positions = DistributePlantPositions(plants, gardenArea);

        for (int i = 0; i < plants.Count; i++)
        {
            var plant = plants[i];
            var indoorStartIdx = (lastFrostIdx - 2 + 12) % 12;
            var transplantIdx = lastFrostIdx;
            var directSowIdx = (lastFrostIdx + 1) % 12;
            var harvestStartIdx = (transplantIdx + (plant.DaysToMaturity / 30)) % 12;
            var harvestEndIdx = (harvestStartIdx + 2) % 12;

            var item = new PlantingScheduleItem
            {
                Plant = plant,
                StartIndoorsMonth = months[indoorStartIdx],
                TransplantMonth = months[transplantIdx],
                DirectSowMonth = months[directSowIdx],
                HarvestStartMonth = months[harvestStartIdx],
                HarvestEndMonth = months[harvestEndIdx],
                Notes = BuildNotes(plant, zone),
                RecommendedPlotPosition = i < positions.Count ? positions[i] : null
            };

            schedule.Add(item);
        }

        return schedule;
    }

    private static string BuildNotes(Plant plant, ClimateZone zone)
    {
        var notes = new List<string>();
        if (plant.WaterRequirement == "High")
            notes.Add("Requires consistent moisture; consider drip irrigation.");
        if (plant.SunRequirement == "Full Sun")
            notes.Add("Plant in the sunniest part of your garden.");
        if (plant.Id == "tomato")
            notes.Add("Plant alongside basil for pest deterrence.");
        if (plant.Id == "basil")
            notes.Add("Great companion for tomatoes; pinch flowers to extend harvest.");
        if (int.TryParse(zone.ZoneCode, out var zoneNum) && zoneNum <= 4)
            notes.Add("Consider using a cold frame or row cover in early spring.");
        return string.Join(" ", notes);
    }

    private static List<LatLng> DistributePlantPositions(List<Plant> plants, GardenArea gardenArea)
    {
        var positions = new List<LatLng>();
        if (gardenArea.Polygon.Count < 3) return positions;

        var centerLat = gardenArea.Polygon.Average(p => p.Lat);
        var centerLng = gardenArea.Polygon.Average(p => p.Lng);

        double latSpan = gardenArea.Polygon.Max(p => p.Lat) - gardenArea.Polygon.Min(p => p.Lat);
        double lngSpan = gardenArea.Polygon.Max(p => p.Lng) - gardenArea.Polygon.Min(p => p.Lng);

        int cols = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(plants.Count)));
        int rows = (int)Math.Ceiling((double)plants.Count / cols);

        for (int i = 0; i < plants.Count; i++)
        {
            int row = i / cols;
            int col = i % cols;
            double lat = centerLat - latSpan / 3 + (row * latSpan / (rows + 1));
            double lng = centerLng - lngSpan / 3 + (col * lngSpan / (cols + 1));
            positions.Add(new LatLng { Lat = lat, Lng = lng });
        }

        return positions;
    }

    /// <summary>
    /// Efficiently shuffles a list and takes the first N items using Random.Shared.
    /// </summary>
    private static List<Plant> ShuffleAndTake(List<Plant> items, int count)
    {
        if (items.Count <= count)
            return new List<Plant>(items);

        var shuffled = new List<Plant>(items);
        var random = Random.Shared;
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int randomIndex = random.Next(i + 1);
            (shuffled[i], shuffled[randomIndex]) = (shuffled[randomIndex], shuffled[i]);
        }

        return shuffled.Take(count).ToList();
    }
}
