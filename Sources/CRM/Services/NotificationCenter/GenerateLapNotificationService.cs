namespace CRM.Services.NotificationCenter;

using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Criteria.GenerateLapNotification;
using CRM.Models.MessageNotificationCenter;
using CRM.Services.Common;
using Newtonsoft.Json;

public interface IGenerateLapNotificationService
{
    Task<IEnumerable<SettlementRemotingRefInfo>> GetPrepareGenLapNotificationAsync(string environment);

    Task InsertGenLapResultAmendmentAsync(IEnumerable<SettlementRemotingRefInfo> jSRemotingRef, string environment);
}

public class GenerateLapNotificationService(IBaseService baseService) : IGenerateLapNotificationService
{
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