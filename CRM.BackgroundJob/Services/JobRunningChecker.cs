namespace Fanex.CRM.BackgroundJob.Services
{
    using System.Threading.Tasks;
    using Fanex.Caching;

    public interface IJobRunningChecker
    {
        Task<bool> IsRunning(string jobName);

        Task MarkRunning(string jobName);

        Task ClearRunning(string jobName);
    }

    public class JobRunningChecker : IJobRunningChecker
    {
        private readonly ICacheService cacheService;

        public JobRunningChecker(ICacheService cacheService)
        {
            this.cacheService = cacheService;
        }

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
            return string.Format("RunningJobs:{0}", jobName);
        }
    }
}