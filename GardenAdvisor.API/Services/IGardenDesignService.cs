namespace GardenAdvisor.API.Services;
using GardenAdvisor.API.Models;

public interface IGardenDesignService
{
    Task<GardenDesignResult> DesignGardenAsync(GardenRequest request, IProgress<string> progress, CancellationToken cancellationToken = default);
}
