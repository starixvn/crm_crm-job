namespace CRM.Services.AutoChatBotMessage;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Criteria.ChatBot_ArchivingDA;
using CRM.Extensions.Xml;
using CRM.Models.ChatBot;
using CRM.Services.Common;

public interface IChatBotArchivingDAService
{
    Task<IEnumerable<ChatbotArchivingDAInfo>> GetChatbotArchivingDASettingInfoAsync(long LastLogId, int BatchSize);

    Task<ValueTuple<CRM.Models.Common.CommandResult, ImportArchivedAccountsCommand>> ImportArchivedAccountsAsync(IEnumerable<ChatbotArchivingDAInfo> archivingDAInfo, string environment);
}

public class ChatBotArchivingDAService(IBaseService baseService) : IChatBotArchivingDAService
{
    public async Task<IEnumerable<ChatbotArchivingDAInfo>> GetChatbotArchivingDASettingInfoAsync(long LastLogId, int BatchSize)
    {
        var criteria = new GETChatbotArchivedAccountsCriteria
        {
            LastLogId = LastLogId,
            BatchSize = BatchSize
        };
        return await baseService.GetDataAsync<ChatbotArchivingDAInfo>(criteria);
    }

    public async Task<ValueTuple<CRM.Models.Common.CommandResult, ImportArchivedAccountsCommand>> ImportArchivedAccountsAsync(IEnumerable<ChatbotArchivingDAInfo> archivingDAInfo, string environment)
    {
        var command = new ImportArchivedAccountsCommand
        {
            XmlData = archivingDAInfo.ToXML("root").ToString(),
            Environment = environment
        };
        var commandResult = await baseService.ExecuteNonQueryAsync(command);
        return new ValueTuple<CRM.Models.Common.CommandResult, ImportArchivedAccountsCommand>(commandResult, command);
    }
}