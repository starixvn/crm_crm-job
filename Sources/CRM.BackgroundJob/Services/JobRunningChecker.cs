namespace CRM.BackgroundJob.Services;

using System.Threading.Tasks;
using Fanex.Caching;

public interface IJobRunningChecker
{
    Task<bool> IsRunning(string jobName);

    Task MarkRunning(string jobName);

    Task ClearRunning(string jobName);
}

public class JobRunningChecker(ICacheService cacheService) : IJobRunningChecker
{
    public async Task<bool> IsRunning(string jobName)
    {
        return await cacheService.GetAsync<bool>(GetKey(jobName));
    }

    public async Task MarkRunning(string jobName)
    {
        await cacheService.SetAsync(GetKey(jobName), true);
    }

    public async Task ClearRunning(string jobName)
    {
        await cacheService.SetAsync(GetKey(jobName), false);
    }

    private static string GetKey(string jobName)
    {
        return $"RunningJobs:{jobName}";
    }
}