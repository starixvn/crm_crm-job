namespace CRM.Services.AutoChatBotMessage;

using CRM.Criteria.ChatBotRebateRetention;
using CRM.Extensions.Xml;
using CRM.Models.ChatBotRebateRetention;
using CRM.Services.Common;
using Models.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IChatBotRebateRetentionService
{
    Task<(IEnumerable<ChatBotRebateRetentionInfo>, string)> GetRebateRetentionInfoAsync(string lastLogSid);

    Task<(CommandResult, ImportRebateRetentionCommand)> ImportRebateRetentionCommand(IEnumerable<ChatBotRebateRetentionInfo> rrMessages,
        string newLogSIDs, string environment);
}

public class ChatBotRebateRetentionService(IBaseService baseService) : IChatBotRebateRetentionService
{
    public async Task<(IEnumerable<ChatBotRebateRetentionInfo>, string)> GetRebateRetentionInfoAsync(string lastLogSid)
    {
        var criteria = new GetRebateRetentionInfoCriteria
        {
            LastLogSID = lastLogSid
        };
        var result = await baseService.GetDataAsync<ChatBotRebateRetentionInfo>(criteria);
        return (result, criteria.NewLogSID);
    }

    public async Task<(CommandResult, ImportRebateRetentionCommand)> ImportRebateRetentionCommand(IEnumerable<ChatBotRebateRetentionInfo> rrMessages,
        string newLogSIDs, string environment)
    {
        var command = new ImportRebateRetentionCommand
        {
            XmlData = rrMessages.ToXML("root").ToString(),
            NewLogSID = newLogSIDs,
            Environment = environment
        };
        var commandResult = await baseService.ExecuteNonQueryAsync(command);
        return (commandResult, command);
    }
}