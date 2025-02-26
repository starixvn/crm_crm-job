namespace CRM.Services.NotificationCenter;

using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Criteria.GenerateResultAmendment;
using CRM.Models.MessageNotificationCenter;
using CRM.Services.Common;
using Newtonsoft.Json;

public interface IGenerateResultAmendmentService
{
    Task<IEnumerable<SettlementRemotingRefInfo>> GetPrepareGenNotificationAsync(string environment);

    Task InsertGenResultAmendmentAsync(string environment, IEnumerable<SettlementRemotingRefInfo> jSRemotingRef);
}

public class GenerateResultAmendmentService(IBaseService baseService) : IGenerateResultAmendmentService
{
    public async Task<IEnumerable<SettlementRemotingRefInfo>> GetPrepareGenNotificationAsync(string environment)
    {
        var criteria = new GetPrepareGenNotificationCriteria
        {
            Environment = environment
        };
        return await baseService.GetDataAsync<SettlementRemotingRefInfo>(criteria);
    }

    public async Task InsertGenResultAmendmentAsync(string environment, IEnumerable<SettlementRemotingRefInfo> jSRemotingRef)
    {
        var command = new InsertGenResultAmendmentCommand
        {
            Environment = environment,
            JSRemotingRef = JsonConvert.SerializeObject(jSRemotingRef)
        };
        await baseService.ExecuteNonQueryAsync(command);
    }
}