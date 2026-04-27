namespace GardenAdvisor.API.Services;
using GardenAdvisor.API.Models;

public class PlantService : IPlantService
{
    private readonly List<PlantCategory> _categories;
    private readonly List<Plant> _plants;

    public PlantService()
    {
        _categories = new List<PlantCategory>
        {
            new() { Id = "vegetables", Name = "Vegetables", Description = "Edible plants grown for their roots, stems, leaves, or fruits.", Icon = "🥦" },
            new() { Id = "fruits", Name = "Fruits", Description = "Sweet and fleshy plant products, including berries and melons.", Icon = "🍓" },
            new() { Id = "flowers", Name = "Flowers & Herbs", Description = "Ornamental plants and aromatic herbs that enhance the garden.", Icon = "🌸" }
        };

        _plants = new List<Plant>
        {
            // Vegetables
            new()
            {
                Id = "tomato", Name = "Tomato", Category = "vegetables",
                Description = "A warm-season crop producing juicy red fruits. One of the most popular garden vegetables.",
                GrowingZones = new() { "3","4","5","6","7","8","9","10" },
                DaysToMaturity = 75, SunRequirement = "Full Sun",
                WaterRequirement = "Medium", SpacingMeters = 0.6
            },
            new()
            {
                Id = "carrot", Name = "Carrot", Category = "vegetables",
                Description = "A root vegetable that thrives in loose, deep soil. Available in orange, purple, and yellow varieties.",
                GrowingZones = new() { "3","4","5","6","7","8","9" },
                DaysToMaturity = 70, SunRequirement = "Full Sun",
                WaterRequirement = "Medium", SpacingMeters = 0.08
            },
            new()
            {
                Id = "lettuce", Name = "Lettuce", Category = "vegetables",
                Description = "A cool-season leafy green ideal for salads. Grows quickly and can be harvested multiple times.",
                GrowingZones = new() { "4","5","6","7","8","9" },
                DaysToMaturity = 45, SunRequirement = "Partial Shade",
                WaterRequirement = "Medium", SpacingMeters = 0.25
            },
            new()
            {
                Id = "spinach", Name = "Spinach", Category = "vegetables",
                Description = "A nutrient-rich leafy green that prefers cool temperatures.",
                GrowingZones = new() { "3","4","5","6","7","8" },
                DaysToMaturity = 40, SunRequirement = "Partial Shade",
                WaterRequirement = "Medium", SpacingMeters = 0.15
            },
            new()
            {
                Id = "cucumber", Name = "Cucumber", Category = "vegetables",
                Description = "A warm-season vine producing crisp, refreshing fruits.",
                GrowingZones = new() { "4","5","6","7","8","9","10" },
                DaysToMaturity = 60, SunRequirement = "Full Sun",
                WaterRequirement = "High", SpacingMeters = 0.5
            },
            new()
            {
                Id = "bell-pepper", Name = "Bell Pepper", Category = "vegetables",
                Description = "A sweet, colorful pepper that thrives in warm climates.",
                GrowingZones = new() { "5","6","7","8","9","10" },
                DaysToMaturity = 80, SunRequirement = "Full Sun",
                WaterRequirement = "Medium", SpacingMeters = 0.45
            },
            new()
            {
                Id = "zucchini", Name = "Zucchini", Category = "vegetables",
                Description = "A prolific summer squash that produces abundantly with minimal care.",
                GrowingZones = new() { "3","4","5","6","7","8","9","10" },
                DaysToMaturity = 50, SunRequirement = "Full Sun",
                WaterRequirement = "Medium", SpacingMeters = 0.9
            },
            new()
            {
                Id = "green-bean", Name = "Green Bean", Category = "vegetables",
                Description = "Easy-to-grow bush or pole beans producing tender pods.",
                GrowingZones = new() { "3","4","5","6","7","8","9","10" },
                DaysToMaturity = 55, SunRequirement = "Full Sun",
                WaterRequirement = "Medium", SpacingMeters = 0.15
            },
            new()
            {
                Id = "pea", Name = "Pea", Category = "vegetables",
                Description = "A cool-season climber with sweet pods. Great for spring and fall gardens.",
                GrowingZones = new() { "3","4","5","6","7","8" },
                DaysToMaturity = 60, SunRequirement = "Full Sun",
                WaterRequirement = "Medium", SpacingMeters = 0.1
            },
            new()
            {
                Id = "radish", Name = "Radish", Category = "vegetables",
                Description = "One of the fastest-growing vegetables. Ready in as little as 25 days.",
                GrowingZones = new() { "2","3","4","5","6","7","8","9" },
                DaysToMaturity = 28, SunRequirement = "Full Sun",
                WaterRequirement = "Medium", SpacingMeters = 0.08
            },
            new()
            {
                Id = "kale", Name = "Kale", Category = "vegetables",
                Description = "A cold-hardy superfood that can survive light frosts and grows into late autumn.",
                GrowingZones = new() { "2","3","4","5","6","7","8","9" },
                DaysToMaturity = 55, SunRequirement = "Full Sun",
                WaterRequirement = "Medium", SpacingMeters = 0.45
            },
            new()
            {
                Id = "broccoli", Name = "Broccoli", Category = "vegetables",
                Description = "A cool-season brassica that produces nutritious heads and side shoots.",
                GrowingZones = new() { "3","4","5","6","7","8","9" },
                DaysToMaturity = 80, SunRequirement = "Full Sun",
                WaterRequirement = "Medium", SpacingMeters = 0.5
            },

            // Fruits
            new()
            {
                Id = "strawberry", Name = "Strawberry", Category = "fruits",
                Description = "A sweet, low-growing perennial producing bright red berries.",
                GrowingZones = new() { "3","4","5","6","7","8","9","10" },
                DaysToMaturity = 90, SunRequirement = "Full Sun",
                WaterRequirement = "Medium", SpacingMeters = 0.3
            },
            new()
            {
                Id = "blueberry", Name = "Blueberry", Category = "fruits",
                Description = "A long-lived perennial shrub producing sweet, antioxidant-rich berries.",
                GrowingZones = new() { "4","5","6","7","8","9" },
                DaysToMaturity = 365, SunRequirement = "Full Sun",
                WaterRequirement = "Medium", SpacingMeters = 1.5
            },
            new()
            {
                Id = "raspberry", Name = "Raspberry", Category = "fruits",
                Description = "A thorny cane fruit producing delicate, sweet-tart berries.",
                GrowingZones = new() { "3","4","5","6","7","8" },
                DaysToMaturity = 365, SunRequirement = "Full Sun",
                WaterRequirement = "Medium", SpacingMeters = 0.6
            },
            new()
            {
                Id = "watermelon", Name = "Watermelon", Category = "fruits",
                Description = "A sprawling vine producing large, refreshing summer melons.",
                GrowingZones = new() { "6","7","8","9","10","11" },
                DaysToMaturity = 90, SunRequirement = "Full Sun",
                WaterRequirement = "High", SpacingMeters = 1.8
            },
            new()
            {
                Id = "cantaloupe", Name = "Cantaloupe", Category = "fruits",
                Description = "A sweet, musky melon that loves heat and sun.",
                GrowingZones = new() { "5","6","7","8","9","10" },
                DaysToMaturity = 85, SunRequirement = "Full Sun",
                WaterRequirement = "High", SpacingMeters = 1.5
            },
            new()
            {
                Id = "pumpkin", Name = "Pumpkin", Category = "fruits",
                Description = "A large vine squash perfect for fall harvest and decoration.",
                GrowingZones = new() { "3","4","5","6","7","8","9","10" },
                DaysToMaturity = 100, SunRequirement = "Full Sun",
                WaterRequirement = "High", SpacingMeters = 1.8
            },

            // Flowers & Herbs
            new()
            {
                Id = "sunflower", Name = "Sunflower", Category = "flowers",
                Description = "A tall, cheerful annual that tracks the sun and attracts pollinators.",
                GrowingZones = new() { "3","4","5","6","7","8","9","10" },
                DaysToMaturity = 80, SunRequirement = "Full Sun",
                WaterRequirement = "Low", SpacingMeters = 0.6
            },
            new()
            {
                Id = "marigold", Name = "Marigold", Category = "flowers",
                Description = "A vibrant companion plant that deters pests and attracts beneficial insects.",
                GrowingZones = new() { "2","3","4","5","6","7","8","9","10" },
                DaysToMaturity = 50, SunRequirement = "Full Sun",
                WaterRequirement = "Low", SpacingMeters = 0.3
            },
            new()
            {
                Id = "lavender", Name = "Lavender", Category = "flowers",
                Description = "A fragrant perennial herb with purple blooms, beloved by pollinators.",
                GrowingZones = new() { "5","6","7","8","9","10" },
                DaysToMaturity = 90, SunRequirement = "Full Sun",
                WaterRequirement = "Low", SpacingMeters = 0.6
            },
            new()
            {
                Id = "zinnia", Name = "Zinnia", Category = "flowers",
                Description = "A heat-loving annual producing vivid flowers that attract butterflies.",
                GrowingZones = new() { "3","4","5","6","7","8","9","10" },
                DaysToMaturity = 60, SunRequirement = "Full Sun",
                WaterRequirement = "Low", SpacingMeters = 0.3
            },
            new()
            {
                Id = "rose", Name = "Rose", Category = "flowers",
                Description = "The classic garden flower available in hundreds of varieties and colors.",
                GrowingZones = new() { "4","5","6","7","8","9" },
                DaysToMaturity = 60, SunRequirement = "Full Sun",
                WaterRequirement = "Medium", SpacingMeters = 1.0
            },
            new()
            {
                Id = "basil", Name = "Basil", Category = "flowers",
                Description = "A fragrant culinary herb that repels pests and enhances tomato flavor when planted nearby.",
                GrowingZones = new() { "5","6","7","8","9","10" },
                DaysToMaturity = 30, SunRequirement = "Full Sun",
                WaterRequirement = "Medium", SpacingMeters = 0.3
            }
        };
    }

    public List<PlantCategory> GetCategories() => _categories;

    public List<Plant> GetPlantsByCategory(string categoryId) =>
        _plants.Where(p => p.Category.Equals(categoryId, StringComparison.OrdinalIgnoreCase)).ToList();

    public Plant? GetPlantById(string id) =>
        _plants.FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    public List<Plant> GetPlantsByIds(List<string> ids) =>
        _plants.Where(p => ids.Contains(p.Id, StringComparer.OrdinalIgnoreCase)).ToList();
}
