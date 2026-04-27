using GardenAdvisor.API.Models;

namespace GardenAdvisor.API.Services;

public interface IGardenPlanRepository
{
    Task SavePlanAsync(GardenPlan plan, CancellationToken cancellationToken = default);
    Task<GardenPlan?> GetPlanBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<GardenPlan>> GetPlansByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}
