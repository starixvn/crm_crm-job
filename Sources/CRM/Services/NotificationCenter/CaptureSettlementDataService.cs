namespace CRM.Services.NotificationCenter;

using CRM.Criteria.CaptureSettlementData;
using CRM.Models.MessageNotificationCenter;
using CRM.Services.Common;
using Models.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ICaptureSettlementDataService
{
    Task<IEnumerable<SiteDbConfig>> GetInitialSiteDbConfigsAsync(string environment);

    Task<IEnumerable<SettleResultHistory>> GetSessionByDateAsync(SiteDbConfig siteDbConfig);

    Task InsertDataLoadAsync(string environment, string jsonSettleResultHistory);
}

public class CaptureSettlementDataService(IBaseService baseService) : ICaptureSettlementDataService
{
    public async Task<IEnumerable<SiteDbConfig>> GetInitialSiteDbConfigsAsync(string environment)
    {
        var criteria = new GetInitialSiteDBConfigCriteria
        {
            Environment = environment
        };
        return await baseService.GetDataAsync<SiteDbConfig>(criteria);
    }

    public async Task<IEnumerable<SettleResultHistory>> GetSessionByDateAsync(SiteDbConfig siteDbConfig)
    {
        var criteria = new GetSessionByDateCriteria
        {
            Environment = siteDbConfig.Environment,
            FDTime = siteDbConfig.FDTime,
            SiteDBGroupID = siteDbConfig.SiteDBGroupID
        };
        return await baseService.GetDataByIpConfigAsync<SettleResultHistory>(siteDbConfig.AGIP, siteDbConfig.LinkedServerName, criteria);
    }

    public async Task InsertDataLoadAsync(string environment, string jsonSettleResultHistory)
    {
        var command = new InsertDataLoadCommand
        {
            Environment = environment,
            JSSettlResultHist = jsonSettleResultHistory
        };
        await baseService.ExecuteNonQueryAsync(command);
    }
}