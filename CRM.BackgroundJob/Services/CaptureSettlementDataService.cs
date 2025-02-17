namespace CRM.BackgroundJob.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CRM.BackgroundJob.Core.Models;
    using CRM.BackgroundJob.Core.Services;
    using CRM.BackgroundJob.Criteria.CaptureSettlementData;
    using CRM.BackgroundJob.Models.MessageNotificationCenter;

    public interface ICaptureSettlementDataService
    {
        Task<IEnumerable<SiteDbConfig>> GetInitialSiteDbConfigsAsync(string environment);

        Task<IEnumerable<SettlResultHistory>> GetSessionByDateAsync(SiteDbConfig siteDbConfig);

        Task InsertDataLoadAsync(string environment, string JSSettlResultHist);
    }

    public class CaptureSettlementDataService : ICaptureSettlementDataService
    {
        private readonly IBaseService baseService;

        public CaptureSettlementDataService(IBaseService baseService)
        {
            this.baseService = baseService;
        }

        public async Task<IEnumerable<SiteDbConfig>> GetInitialSiteDbConfigsAsync(string environment)
        {
            var criteria = new GetInitalSiteDBConfigCriteria
            {
                Environment = environment
            };
            return await baseService.GetDataAsync<SiteDbConfig>(criteria);
        }

        public async Task<IEnumerable<SettlResultHistory>> GetSessionByDateAsync(SiteDbConfig siteDbConfig)
        {
            var criteria = new GetSessionByDateCriteria
            {
                Environment = siteDbConfig.Environment,
                FDTime = siteDbConfig.FDTime,
                SiteDBGroupID = siteDbConfig.SiteDBGroupID
            };
            return await baseService.GetDataByIpConfigAsync<SettlResultHistory>(siteDbConfig.AGIP, siteDbConfig.LinkedServerName, criteria);
        }

        public async Task InsertDataLoadAsync(string environment, string JSSettlResultHist)
        {
            var command = new InsertDataLoadCommand
            {
                Environment = environment,
                JSSettlResultHist = JSSettlResultHist
            };
            await baseService.ExecuteNonQueryAsync(command);
        }
    }
}