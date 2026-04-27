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

        var schedule = BuildSchedule(suitablePlants, climateZone, request.GardenArea, request.Address.Latitude);

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

    private List<PlantingScheduleItem> BuildSchedule(List<Plant> plants, ClimateZone zone, GardenArea gardenArea, double latitude)
    {
        var schedule = new List<PlantingScheduleItem>();
        var months = new[] { "January","February","March","April","May","June","July","August","September","October","November","December" };
        var lastFrostIdx = Array.IndexOf(months, zone.LastFrostMonth);
        if (lastFrostIdx < 0) lastFrostIdx = 3; // default April

        var positions = DistributePlantPositions(plants, gardenArea, latitude);

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
                Notes = BuildNotes(plant, zone, plants),
                RecommendedPlotPosition = i < positions.Count ? positions[i] : null
            };

            schedule.Add(item);
        }

        return schedule;
    }

    private static string BuildNotes(Plant plant, ClimateZone zone, IReadOnlyList<Plant> selectedPlants)
    {
        var notes = new List<string>();

        if (plant.WaterRequirement == "High")
            notes.Add("Requires consistent moisture; consider drip irrigation.");
        if (plant.SunRequirement == "Full Sun")
            notes.Add("Plant in the sunniest part of your garden.");
        if (int.TryParse(zone.ZoneCode, out var zoneNum) && zoneNum <= 4)
            notes.Add("Consider using a cold frame or row cover in early spring.");

        var presentCompanions = plant.CompanionPlants
            .Select(id => selectedPlants.FirstOrDefault(p => p.Id == id))
            .Where(p => p is not null)
            .Select(p => p!.Name)
            .ToList();
        if (presentCompanions.Count > 0)
            notes.Add($"Plant near: {string.Join(", ", presentCompanions)} for mutual benefit.");

        var presentIncompatibles = plant.IncompatiblePlants
            .Select(id => selectedPlants.FirstOrDefault(p => p.Id == id))
            .Where(p => p is not null)
            .Select(p => p!.Name)
            .ToList();
        if (presentIncompatibles.Count > 0)
            notes.Add($"Keep away from: {string.Join(", ", presentIncompatibles)}.");

        if (plant.HeightMeters >= 1.0)
            notes.Add($"Tall plant ({plant.HeightMeters:0.0}m); placed at the back of your garden to avoid shading shorter plants.");

        return string.Join(" ", notes);
    }

    private static List<LatLng> DistributePlantPositions(List<Plant> plants, GardenArea gardenArea, double latitude)
    {
        var positions = plants.Select(_ => new LatLng()).ToList();
        if (gardenArea.Polygon.Count < 3 || plants.Count == 0) return positions;

        double minLat = gardenArea.Polygon.Min(p => p.Lat);
        double maxLat = gardenArea.Polygon.Max(p => p.Lat);
        double minLng = gardenArea.Polygon.Min(p => p.Lng);
        double maxLng = gardenArea.Polygon.Max(p => p.Lng);
        double latSpan = maxLat - minLat;
        double lngSpan = maxLng - minLng;

        // In the Northern Hemisphere the sun moves through the south, so tall plants
        // belong at the northern (high-latitude) end of the garden to avoid shading
        // shorter plants. The logic inverts in the Southern Hemisphere.
        bool northernHemisphere = latitude >= 0;

        int n = plants.Count;
        int cols = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(n)));
        int rows = (int)Math.Ceiling((double)n / cols);

        // Build a layout order: tallest plants first (row 0 = back of garden),
        // with companions clustered together and incompatibles pushed apart.
        var orderedIndices = BuildLayoutOrder(plants);

        const double margin = 0.1;
        double usableLat = latSpan * (1.0 - 2 * margin);
        double usableLng = lngSpan * (1.0 - 2 * margin);

        for (int layoutPos = 0; layoutPos < orderedIndices.Count; layoutPos++)
        {
            int originalIdx = orderedIndices[layoutPos];
            int row = layoutPos / cols;
            int col = layoutPos % cols;
            int colsInThisRow = Math.Min(cols, n - row * cols);

            double rowFraction = rows > 1 ? (double)row / (rows - 1) : 0.5;
            double colFraction = colsInThisRow > 1 ? (double)col / (colsInThisRow - 1) : 0.5;

            // Row 0 is placed at the back of the garden (away from the sun path).
            double lat = northernHemisphere
                ? maxLat - latSpan * margin - rowFraction * usableLat   // back = north
                : minLat + latSpan * margin + rowFraction * usableLat;  // back = south
            double lng = minLng + lngSpan * margin + colFraction * usableLng;

            positions[originalIdx] = new LatLng { Lat = lat, Lng = lng };
        }

        return positions;
    }

    // Returns plant indices in layout order:
    // 1. Sorted by height descending (tallest → back row).
    // 2. Companions clustered together within that order.
    // 3. Incompatible pairs swapped to different rows where possible.
    private static List<int> BuildLayoutOrder(List<Plant> plants)
    {
        var byHeight = plants
            .Select((p, i) => (plant: p, index: i))
            .OrderByDescending(x => x.plant.HeightMeters)
            .ToList();

        var result = new List<int>(plants.Count);
        var placed = new HashSet<int>();

        foreach (var (plant, index) in byHeight)
        {
            if (placed.Contains(index)) continue;
            result.Add(index);
            placed.Add(index);

            // Place this plant's companions immediately after it (in height order)
            // so they land in the same or an adjacent row.
            foreach (var (compPlant, compIdx) in byHeight)
            {
                if (!placed.Contains(compIdx) && plant.CompanionPlants.Contains(compPlant.Id))
                {
                    result.Add(compIdx);
                    placed.Add(compIdx);
                }
            }
        }

        SeparateIncompatibles(plants, result);
        return result;
    }

    // If two incompatible plants share the same grid row, swap one of them with
    // a plant in a different row that is not incompatible with either neighbour.
    private static void SeparateIncompatibles(List<Plant> plants, List<int> order)
    {
        int n = order.Count;
        int cols = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(n)));

        for (int i = 0; i < n; i++)
        {
            var plantA = plants[order[i]];
            int rowA = i / cols;

            for (int j = i + 1; j < n; j++)
            {
                var plantB = plants[order[j]];
                if (j / cols != rowA) continue; // skip plants already in a different row
                if (!plantA.IncompatiblePlants.Contains(plantB.Id)) continue;

                // Find a candidate in a different row to swap plantB with
                for (int k = n - 1; k >= 0; k--)
                {
                    if (k / cols == rowA) continue; // must be a different row
                    var plantK = plants[order[k]];
                    if (plantA.IncompatiblePlants.Contains(plantK.Id)) continue;
                    if (plantB.IncompatiblePlants.Contains(plantK.Id)) continue;
                    (order[j], order[k]) = (order[k], order[j]);
                    break;
                }
            }
        }
    }

    private static List<Plant> ShuffleAndTake(List<Plant> items, int count)
    {
        if (items.Count <= count)
            return new List<Plant>(items);

        // Partial Fisher-Yates: only shuffle the first `count` positions
        var shuffled = new List<Plant>(items);
        var random = Random.Shared;
        for (int i = 0; i < count; i++)
        {
            int j = random.Next(i, shuffled.Count);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        return shuffled.GetRange(0, count);
    }
}
