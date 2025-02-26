namespace CRM.Services.AutoFeedback;

using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Criteria.AutoFbRebateRetention;
using CRM.Criteria.ChatBotRebateRetention;
using CRM.Extensions.Xml;
using CRM.Models.ChatBotRebateRetention;
using CRM.Services.Common;

public interface IAutoFbRebateRetentionService
{
    Task<(IEnumerable<ChatBotRebateRetentionInfo>, string)> GetRebateRetentionInfoAsync(string lastLogSId);

    Task<(CRM.Models.Common.CommandResult, ImportAutoFbRebateRetentionCommand)> ImportRebateRetentionCommand(IEnumerable<ChatBotRebateRetentionInfo> rrMessages, string newLogSIDs, string environment);
}

public class AutoFbRebateRetentionService(IBaseService baseService) : IAutoFbRebateRetentionService
{
    public async Task<(IEnumerable<ChatBotRebateRetentionInfo>, string)> GetRebateRetentionInfoAsync(string lastLogSId)
    {
        var criteria = new GetRebateRetentionInfoCriteria
        {
            LastLogSID = lastLogSId
        };
        var result = await baseService.GetDataAsync<ChatBotRebateRetentionInfo>(criteria);
        return (result, criteria.NewLogSID);
    }

    public async Task<(CRM.Models.Common.CommandResult, ImportAutoFbRebateRetentionCommand)> ImportRebateRetentionCommand(IEnumerable<ChatBotRebateRetentionInfo> rrMessages, string newLogSIDs, string environment)
    {
        var command = new ImportAutoFbRebateRetentionCommand
        {
            XmlData = rrMessages.ToXML("root").ToString(),
            NewLogSID = newLogSIDs,
            Environment = environment
        };
        var commandResult = await baseService.ExecuteNonQueryAsync(command);
        return (commandResult, command);
    }
}