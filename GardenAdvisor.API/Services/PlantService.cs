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
                WaterRequirement = "Medium", SpacingMeters = 0.6,
                HeightMeters = 1.2,
                CompanionPlants = new() { "basil", "marigold", "carrot" },
                IncompatiblePlants = new() { "broccoli", "kale" }
            },
            new()
            {
                Id = "carrot", Name = "Carrot", Category = "vegetables",
                Description = "A root vegetable that thrives in loose, deep soil. Available in orange, purple, and yellow varieties.",
                GrowingZones = new() { "3","4","5","6","7","8","9" },
                DaysToMaturity = 70, SunRequirement = "Full Sun",
                WaterRequirement = "Medium", SpacingMeters = 0.08,
                HeightMeters = 0.3,
                CompanionPlants = new() { "tomato", "pea", "radish", "lettuce" },
                IncompatiblePlants = new()
            },
            new()
            {
                Id = "lettuce", Name = "Lettuce", Category = "vegetables",
                Description = "A cool-season leafy green ideal for salads. Grows quickly and can be harvested multiple times.",
                GrowingZones = new() { "4","5","6","7","8","9" },
                DaysToMaturity = 45, SunRequirement = "Partial Shade",
                WaterRequirement = "Medium", SpacingMeters = 0.25,
                HeightMeters = 0.3,
                CompanionPlants = new() { "radish", "spinach", "carrot", "strawberry" },
                IncompatiblePlants = new() { "sunflower" }
            },
            new()
            {
                Id = "spinach", Name = "Spinach", Category = "vegetables",
                Description = "A nutrient-rich leafy green that prefers cool temperatures.",
                GrowingZones = new() { "3","4","5","6","7","8" },
                DaysToMaturity = 40, SunRequirement = "Partial Shade",
                WaterRequirement = "Medium", SpacingMeters = 0.15,
                HeightMeters = 0.3,
                CompanionPlants = new() { "lettuce", "radish", "strawberry" },
                IncompatiblePlants = new() { "sunflower" }
            },
            new()
            {
                Id = "cucumber", Name = "Cucumber", Category = "vegetables",
                Description = "A warm-season vine producing crisp, refreshing fruits.",
                GrowingZones = new() { "4","5","6","7","8","9","10" },
                DaysToMaturity = 60, SunRequirement = "Full Sun",
                WaterRequirement = "High", SpacingMeters = 0.5,
                HeightMeters = 1.5,
                CompanionPlants = new() { "radish", "green-bean", "sunflower" },
                IncompatiblePlants = new()
            },
            new()
            {
                Id = "bell-pepper", Name = "Bell Pepper", Category = "vegetables",
                Description = "A sweet, colorful pepper that thrives in warm climates.",
                GrowingZones = new() { "5","6","7","8","9","10" },
                DaysToMaturity = 80, SunRequirement = "Full Sun",
                WaterRequirement = "Medium", SpacingMeters = 0.45,
                HeightMeters = 0.6,
                CompanionPlants = new() { "basil", "marigold", "carrot" },
                IncompatiblePlants = new()
            },
            new()
            {
                Id = "zucchini", Name = "Zucchini", Category = "vegetables",
                Description = "A prolific summer squash that produces abundantly with minimal care.",
                GrowingZones = new() { "3","4","5","6","7","8","9","10" },
                DaysToMaturity = 50, SunRequirement = "Full Sun",
                WaterRequirement = "Medium", SpacingMeters = 0.9,
                HeightMeters = 0.6,
                CompanionPlants = new() { "pea", "marigold" },
                IncompatiblePlants = new() { "watermelon", "cantaloupe", "pumpkin" }
            },
            new()
            {
                Id = "green-bean", Name = "Green Bean", Category = "vegetables",
                Description = "Easy-to-grow bush or pole beans producing tender pods.",
                GrowingZones = new() { "3","4","5","6","7","8","9","10" },
                DaysToMaturity = 55, SunRequirement = "Full Sun",
                WaterRequirement = "Medium", SpacingMeters = 0.15,
                HeightMeters = 0.5,
                CompanionPlants = new() { "cucumber", "pea", "carrot", "radish" },
                IncompatiblePlants = new()
            },
            new()
            {
                Id = "pea", Name = "Pea", Category = "vegetables",
                Description = "A cool-season climber with sweet pods. Great for spring and fall gardens.",
                GrowingZones = new() { "3","4","5","6","7","8" },
                DaysToMaturity = 60, SunRequirement = "Full Sun",
                WaterRequirement = "Medium", SpacingMeters = 0.1,
                HeightMeters = 1.0,
                CompanionPlants = new() { "carrot", "radish", "green-bean" },
                IncompatiblePlants = new()
            },
            new()
            {
                Id = "radish", Name = "Radish", Category = "vegetables",
                Description = "One of the fastest-growing vegetables. Ready in as little as 25 days.",
                GrowingZones = new() { "2","3","4","5","6","7","8","9" },
                DaysToMaturity = 28, SunRequirement = "Full Sun",
                WaterRequirement = "Medium", SpacingMeters = 0.08,
                HeightMeters = 0.2,
                CompanionPlants = new() { "lettuce", "spinach", "cucumber", "carrot", "pea", "green-bean" },
                IncompatiblePlants = new()
            },
            new()
            {
                Id = "kale", Name = "Kale", Category = "vegetables",
                Description = "A cold-hardy superfood that can survive light frosts and grows into late autumn.",
                GrowingZones = new() { "2","3","4","5","6","7","8","9" },
                DaysToMaturity = 55, SunRequirement = "Full Sun",
                WaterRequirement = "Medium", SpacingMeters = 0.45,
                HeightMeters = 0.6,
                CompanionPlants = new() { "marigold", "cucumber" },
                IncompatiblePlants = new() { "tomato" }
            },
            new()
            {
                Id = "broccoli", Name = "Broccoli", Category = "vegetables",
                Description = "A cool-season brassica that produces nutritious heads and side shoots.",
                GrowingZones = new() { "3","4","5","6","7","8","9" },
                DaysToMaturity = 80, SunRequirement = "Full Sun",
                WaterRequirement = "Medium", SpacingMeters = 0.5,
                HeightMeters = 0.6,
                CompanionPlants = new() { "marigold", "cucumber" },
                IncompatiblePlants = new() { "tomato", "strawberry" }
            },

            // Fruits
            new()
            {
                Id = "strawberry", Name = "Strawberry", Category = "fruits",
                Description = "A sweet, low-growing perennial producing bright red berries.",
                GrowingZones = new() { "3","4","5","6","7","8","9","10" },
                DaysToMaturity = 90, SunRequirement = "Full Sun",
                WaterRequirement = "Medium", SpacingMeters = 0.3,
                HeightMeters = 0.2,
                CompanionPlants = new() { "lettuce", "spinach" },
                IncompatiblePlants = new() { "broccoli" }
            },
            new()
            {
                Id = "blueberry", Name = "Blueberry", Category = "fruits",
                Description = "A long-lived perennial shrub producing sweet, antioxidant-rich berries.",
                GrowingZones = new() { "4","5","6","7","8","9" },
                DaysToMaturity = 365, SunRequirement = "Full Sun",
                WaterRequirement = "Medium", SpacingMeters = 1.5,
                HeightMeters = 1.5,
                CompanionPlants = new() { "strawberry" },
                IncompatiblePlants = new()
            },
            new()
            {
                Id = "raspberry", Name = "Raspberry", Category = "fruits",
                Description = "A thorny cane fruit producing delicate, sweet-tart berries.",
                GrowingZones = new() { "3","4","5","6","7","8" },
                DaysToMaturity = 365, SunRequirement = "Full Sun",
                WaterRequirement = "Medium", SpacingMeters = 0.6,
                HeightMeters = 1.5,
                CompanionPlants = new(),
                IncompatiblePlants = new()
            },
            new()
            {
                Id = "watermelon", Name = "Watermelon", Category = "fruits",
                Description = "A sprawling vine producing large, refreshing summer melons.",
                GrowingZones = new() { "6","7","8","9","10","11" },
                DaysToMaturity = 90, SunRequirement = "Full Sun",
                WaterRequirement = "High", SpacingMeters = 1.8,
                HeightMeters = 0.4,
                CompanionPlants = new(),
                IncompatiblePlants = new() { "zucchini", "pumpkin", "cantaloupe" }
            },
            new()
            {
                Id = "cantaloupe", Name = "Cantaloupe", Category = "fruits",
                Description = "A sweet, musky melon that loves heat and sun.",
                GrowingZones = new() { "5","6","7","8","9","10" },
                DaysToMaturity = 85, SunRequirement = "Full Sun",
                WaterRequirement = "High", SpacingMeters = 1.5,
                HeightMeters = 0.4,
                CompanionPlants = new(),
                IncompatiblePlants = new() { "watermelon", "pumpkin", "zucchini" }
            },
            new()
            {
                Id = "pumpkin", Name = "Pumpkin", Category = "fruits",
                Description = "A large vine squash perfect for fall harvest and decoration.",
                GrowingZones = new() { "3","4","5","6","7","8","9","10" },
                DaysToMaturity = 100, SunRequirement = "Full Sun",
                WaterRequirement = "High", SpacingMeters = 1.8,
                HeightMeters = 0.4,
                CompanionPlants = new() { "pea", "radish" },
                IncompatiblePlants = new() { "watermelon", "cantaloupe", "zucchini" }
            },

            // Flowers & Herbs
            new()
            {
                Id = "sunflower", Name = "Sunflower", Category = "flowers",
                Description = "A tall, cheerful annual that tracks the sun and attracts pollinators.",
                GrowingZones = new() { "3","4","5","6","7","8","9","10" },
                DaysToMaturity = 80, SunRequirement = "Full Sun",
                WaterRequirement = "Low", SpacingMeters = 0.6,
                HeightMeters = 2.0,
                CompanionPlants = new() { "cucumber" },
                IncompatiblePlants = new() { "lettuce", "spinach" }
            },
            new()
            {
                Id = "marigold", Name = "Marigold", Category = "flowers",
                Description = "A vibrant companion plant that deters pests and attracts beneficial insects.",
                GrowingZones = new() { "2","3","4","5","6","7","8","9","10" },
                DaysToMaturity = 50, SunRequirement = "Full Sun",
                WaterRequirement = "Low", SpacingMeters = 0.3,
                HeightMeters = 0.3,
                CompanionPlants = new() { "tomato", "basil", "broccoli", "bell-pepper", "zucchini", "kale", "rose" },
                IncompatiblePlants = new()
            },
            new()
            {
                Id = "lavender", Name = "Lavender", Category = "flowers",
                Description = "A fragrant perennial herb with purple blooms, beloved by pollinators.",
                GrowingZones = new() { "5","6","7","8","9","10" },
                DaysToMaturity = 90, SunRequirement = "Full Sun",
                WaterRequirement = "Low", SpacingMeters = 0.6,
                HeightMeters = 0.6,
                CompanionPlants = new() { "rose" },
                IncompatiblePlants = new()
            },
            new()
            {
                Id = "zinnia", Name = "Zinnia", Category = "flowers",
                Description = "A heat-loving annual producing vivid flowers that attract butterflies.",
                GrowingZones = new() { "3","4","5","6","7","8","9","10" },
                DaysToMaturity = 60, SunRequirement = "Full Sun",
                WaterRequirement = "Low", SpacingMeters = 0.3,
                HeightMeters = 0.6,
                CompanionPlants = new() { "marigold" },
                IncompatiblePlants = new()
            },
            new()
            {
                Id = "rose", Name = "Rose", Category = "flowers",
                Description = "The classic garden flower available in hundreds of varieties and colors.",
                GrowingZones = new() { "4","5","6","7","8","9" },
                DaysToMaturity = 60, SunRequirement = "Full Sun",
                WaterRequirement = "Medium", SpacingMeters = 1.0,
                HeightMeters = 1.2,
                CompanionPlants = new() { "lavender", "marigold" },
                IncompatiblePlants = new()
            },
            new()
            {
                Id = "basil", Name = "Basil", Category = "flowers",
                Description = "A fragrant culinary herb that repels pests and enhances tomato flavor when planted nearby.",
                GrowingZones = new() { "5","6","7","8","9","10" },
                DaysToMaturity = 30, SunRequirement = "Full Sun",
                WaterRequirement = "Medium", SpacingMeters = 0.3,
                HeightMeters = 0.5,
                CompanionPlants = new() { "tomato", "bell-pepper" },
                IncompatiblePlants = new()
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
