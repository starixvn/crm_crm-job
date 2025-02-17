namespace CRM.BackgroundJob.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CRM.BackgroundJob.Core.Extensions.Xml;
    using CRM.BackgroundJob.Core.Models;
    using CRM.BackgroundJob.Core.Services;
    using CRM.BackgroundJob.Criteria.ChatBotRebateRetention;
    using CRM.BackgroundJob.Models.ChatBotRebateRetention;

    public interface IChatBotRebateRetentionService
    {
        Task<(IEnumerable<ChatBotRebateRetentionInfo>, string)> GetRebateRetentionInfoAsync(string lastLogSid);

        Task<(CommandResult, ImportRebateRetentionCommand)> ImportRebateRetentionCommand(IEnumerable<ChatBotRebateRetentionInfo> rrMessages, string newLogSIDs, string environment);
    }

    public class ChatBotRebateRetentionService : IChatBotRebateRetentionService
    {
        private readonly IBaseService baseService;

        public ChatBotRebateRetentionService(IBaseService baseService)
        {
            this.baseService = baseService;
        }

        public async Task<(IEnumerable<ChatBotRebateRetentionInfo>, string)> GetRebateRetentionInfoAsync(string lastLogSid)
        {
            var criteria = new GetRebateRetentionInfoCriteria
            {
                LastLogSID = lastLogSid
            };
            var result = await baseService.GetDataAsync<ChatBotRebateRetentionInfo>(criteria);
            return (result, criteria.NewLogSID);
        }

        public async Task<(CommandResult, ImportRebateRetentionCommand)> ImportRebateRetentionCommand(IEnumerable<ChatBotRebateRetentionInfo> rrMessages, string newLogSIDs, string environment)
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
}
