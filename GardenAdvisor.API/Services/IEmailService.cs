namespace GardenAdvisor.API.Services;

public interface IEmailService
{
    Task SendScheduleEmailAsync(string email, string userName, GardenAdvisor.API.Models.GardenDesignResult result);
}
