namespace CRM.Services.NotificationCenter;

using CRM.Criteria.CaptureSettlementData;
using CRM.Models.MessageNotificationCenter;
using CRM.Services.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ICaptureSettlementDataService
{
    Task<IEnumerable<CRM.Models.Common.SiteDbConfig>> GetInitialSiteDbConfigsAsync(string environment);

    Task<IEnumerable<SettleResultHistory>> GetSessionByDateAsync(CRM.Models.Common.SiteDbConfig siteDbConfig);

    Task InsertDataLoadAsync(string environment, string JSSettlResultHist);
}

public class CaptureSettlementDataService(IBaseService baseService) : ICaptureSettlementDataService
{
    public async Task<IEnumerable<CRM.Models.Common.SiteDbConfig>> GetInitialSiteDbConfigsAsync(string environment)
    {
        var criteria = new GetInitialSiteDBConfigCriteria
        {
            Environment = environment
        };
        return await baseService.GetDataAsync<CRM.Models.Common.SiteDbConfig>(criteria);
    }

    public async Task<IEnumerable<SettleResultHistory>> GetSessionByDateAsync(CRM.Models.Common.SiteDbConfig siteDbConfig)
    {
        var criteria = new GetSessionByDateCriteria
        {
            Environment = siteDbConfig.Environment,
            FDTime = siteDbConfig.FDTime,
            SiteDBGroupID = siteDbConfig.SiteDBGroupID
        };
        return await baseService.GetDataByIpConfigAsync<SettleResultHistory>(siteDbConfig.AGIP, siteDbConfig.LinkedServerName, criteria);
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