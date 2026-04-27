using Microsoft.Azure.Cosmos;
using GardenAdvisor.API.Models;

namespace GardenAdvisor.API.Services;

public class CosmosDbGardenPlanRepository : IGardenPlanRepository
{
    private readonly Container _container;

    public CosmosDbGardenPlanRepository(CosmosClient cosmosClient, IConfiguration configuration)
    {
        var databaseName = configuration["CosmosDb:DatabaseName"] ?? "GardenAdvisor";
        var containerName = configuration["CosmosDb:ContainerName"] ?? "GardenPlans";
        _container = cosmosClient.GetContainer(databaseName, containerName);
    }

    public async Task SavePlanAsync(GardenPlan plan, CancellationToken cancellationToken = default)
    {
        await _container.UpsertItemAsync(plan, new PartitionKey(plan.SessionId), cancellationToken: cancellationToken);
    }

    public async Task<GardenPlan?> GetPlanBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.sessionId = @sessionId")
            .WithParameter("@sessionId", sessionId);

        using var iterator = _container.GetItemQueryIterator<GardenPlan>(query);
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            var item = response.FirstOrDefault();
            if (item is not null)
                return item;
        }

        return null;
    }

    public async Task<IEnumerable<GardenPlan>> GetPlansByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.userId = @userId ORDER BY c.createdAt DESC")
            .WithParameter("@userId", userId);

        var plans = new List<GardenPlan>();
        using var iterator = _container.GetItemQueryIterator<GardenPlan>(query);
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            plans.AddRange(response);
        }

        return plans;
    }
}
