using GardenAdvisor.API.Hubs;
using GardenAdvisor.API.Services;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "GardenAdvisor API", Version = "v1", Description = "Backend API for the GardenAdvisor application" });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddHttpClient("nominatim", client =>
{
    client.BaseAddress = new Uri("https://nominatim.openstreetmap.org/");
    client.DefaultRequestHeaders.Add("User-Agent", "GardenAdvisor/1.0 (contact@gardenadvisor.app)");
    client.DefaultRequestHeaders.Add("Accept-Language", "en");
});

builder.Services.AddSingleton<IPlantService, PlantService>();
builder.Services.AddSingleton<IClimateZoneService, ClimateZoneService>();
builder.Services.AddScoped<IGardenDesignService, GardenDesignService>();
builder.Services.AddScoped<IEmailService, EmailService>();

var cosmosEndpoint = builder.Configuration["CosmosDb:AccountEndpoint"];
var cosmosKey = builder.Configuration["CosmosDb:AccountKey"];
if (!string.IsNullOrEmpty(cosmosEndpoint) && !string.IsNullOrEmpty(cosmosKey))
{
builder.Services.AddSingleton<CosmosClient>(_ => new CosmosClient(cosmosEndpoint, cosmosKey,
        new CosmosClientOptions { SerializerOptions = new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase } }));
    builder.Services.AddScoped<IGardenPlanRepository, CosmosDbGardenPlanRepository>();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GardenAdvisor API v1"));
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();
app.MapHub<GardenProcessingHub>("/hubs/garden");

app.Run();
