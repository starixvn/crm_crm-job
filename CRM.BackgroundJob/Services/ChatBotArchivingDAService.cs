namespace CRM.BackgroundJob.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CRM.BackgroundJob.Criteria.ChatBot_ArchivingDA;
    using CRM.BackgroundJob.Models.ChatBot;
    using CRM.BackgroundJob.Core.Extensions.Xml;
    using CRM.BackgroundJob.Core.Models;
    using CRM.BackgroundJob.Core.Services;
    public interface IChatBotArchivingDAService
    {
        Task<IEnumerable<ChatbotArchivingDAInfor>> GetChatbotArchivingDASettingInfoAsync(long LastLogId, int BatchSize);
        Task<ValueTuple<CommandResult, ImportArchivedAccountsCommand>> ImportArchivedAccountsAsync(IEnumerable<ChatbotArchivingDAInfor> archivingDAInfors, string environment);
    }

    public class ChatBotArchivingDAService : IChatBotArchivingDAService
    {
        private readonly IBaseService baseService;

        public ChatBotArchivingDAService(
           IBaseService baseService)
        {
            this.baseService = baseService;
        }

        public async Task<IEnumerable<ChatbotArchivingDAInfor>> GetChatbotArchivingDASettingInfoAsync(long LastLogId, int BatchSize)
        {
            var criteria = new GETChatbotArchivedAccountsCriteria
            {
                LastLogId = LastLogId,
                BatchSize = BatchSize
            };
            return await baseService.GetDataAsync<ChatbotArchivingDAInfor>(criteria);
        }

        public async Task<ValueTuple<CommandResult, ImportArchivedAccountsCommand>> ImportArchivedAccountsAsync(IEnumerable<ChatbotArchivingDAInfor> archivingDAInfors, string environment)
        {
            var command = new ImportArchivedAccountsCommand
            {
                XmlData = archivingDAInfors.ToXML("root").ToString(),
                Environment = environment
            };
            var commandResult = await baseService.ExecuteNonQueryAsync(command);
            return new ValueTuple<CommandResult, ImportArchivedAccountsCommand>(commandResult, command);
        }
    }
}