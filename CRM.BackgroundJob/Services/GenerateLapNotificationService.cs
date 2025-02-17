namespace CRM.BackgroundJob.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CRM.BackgroundJob.Criteria.GenerateLapNotification;
    using CRM.BackgroundJob.Models.MessageNotificationCenter;
    using CRM.BackgroundJob.Core.Services;
    using Newtonsoft.Json;

    public interface IGenerateLapNotificationService
    {
        Task<IEnumerable<SettlementRemotingRefInfo>> GetPrepareGenLapNotificationAsync(string environment);

        Task InsertGenLapResultAmendmentAsync(IEnumerable<SettlementRemotingRefInfo> jSRemotingRef, string environment);
    }

    public class GenerateLapNotificationService : IGenerateLapNotificationService
    {
        private readonly IBaseService baseService;

        public GenerateLapNotificationService(IBaseService baseService)
        {
            this.baseService = baseService;
        }

        public async Task<IEnumerable<SettlementRemotingRefInfo>> GetPrepareGenLapNotificationAsync(string environment)
        {
            var criteria = new GetPrepareGenLapNotificationCriteria
            {
                Environment = environment
            };
            return await baseService.GetDataAsync<SettlementRemotingRefInfo>(criteria);
        }

        public async Task InsertGenLapResultAmendmentAsync(IEnumerable<SettlementRemotingRefInfo> jSRemotingRef, string environment)
        {
            var command = new InsertGenLapResultAmendmentCommand
            {
                JSRemotingRef = JsonConvert.SerializeObject(jSRemotingRef),
                Environment = environment
            };
            await baseService.ExecuteNonQueryAsync(command);
        }
    }
}
