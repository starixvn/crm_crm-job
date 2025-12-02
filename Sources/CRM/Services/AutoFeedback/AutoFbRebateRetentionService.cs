namespace CRM.Services.AutoFeedback;

using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Criteria.AutoFbRebateRetention;
using CRM.Criteria.ChatBotRebateRetention;
using CRM.Extensions.Xml;
using CRM.Models.ChatBotRebateRetention;
using CRM.Services.Common;
using Models.Common;

public interface IAutoFbRebateRetentionService
{
    Task<(IEnumerable<ChatBotRebateRetentionInfo>, string)> GetRebateRetentionInfoAsync(string lastLogSId, int batchSize);

    Task<(CommandResult, ImportAutoFbRebateRetentionCommand)> ImportRebateRetentionCommand(
        IEnumerable<ChatBotRebateRetentionInfo> rrMessages, string newLogSIDs, string environment);
}

public class AutoFbRebateRetentionService(IBaseService baseService) : IAutoFbRebateRetentionService
{
    public async Task<(IEnumerable<ChatBotRebateRetentionInfo>, string)> GetRebateRetentionInfoAsync(string lastLogSId, int batchSize)
    {
        var criteria = new GetRebateRetentionInfoCriteria
        {
            LastLogSID = lastLogSId,
            BatchSize = batchSize
        };
        var result = await baseService.GetDataAsync<ChatBotRebateRetentionInfo>(criteria);
        return (result, criteria.NewLogSID);
    }

    public async Task<(CommandResult, ImportAutoFbRebateRetentionCommand)> ImportRebateRetentionCommand(IEnumerable<ChatBotRebateRetentionInfo> rrMessages,
        string newLogSIDs, string environment)
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