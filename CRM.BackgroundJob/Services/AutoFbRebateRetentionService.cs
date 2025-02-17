namespace CRM.BackgroundJob.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CRM.BackgroundJob.Core.Extensions.Xml;
    using CRM.BackgroundJob.Core.Models;
    using CRM.BackgroundJob.Core.Services;
    using CRM.BackgroundJob.Criteria.AutoFbRebateRetention;
    using CRM.BackgroundJob.Criteria.ChatBotRebateRetention;
    using CRM.BackgroundJob.Models.ChatBotRebateRetention;

    public interface IAutoFbRebateRetentionService
    {
        Task<(IEnumerable<ChatBotRebateRetentionInfo>, string)> GetRebateRetentionInfoAsync(string lastLogSId);

        Task<(CommandResult, ImportAutoFbRebateRetentionCommand)> ImportRebateRetentionCommand(IEnumerable<ChatBotRebateRetentionInfo> rrMessages, string newLogSIDs, string environment);
    }

    public class AutoFbRebateRetentionService : IAutoFbRebateRetentionService
    {
        private readonly IBaseService baseService;

        public AutoFbRebateRetentionService(IBaseService baseService)
        {
            this.baseService = baseService;
        }

        public async Task<(IEnumerable<ChatBotRebateRetentionInfo>, string)> GetRebateRetentionInfoAsync(string lastLogSId)
        {
            var criteria = new GetRebateRetentionInfoCriteria
            {
                LastLogSID = lastLogSId
            };
            var result = await baseService.GetDataAsync<ChatBotRebateRetentionInfo>(criteria);
            return (result, criteria.NewLogSID);
        }

        public async Task<(CommandResult, ImportAutoFbRebateRetentionCommand)> ImportRebateRetentionCommand(IEnumerable<ChatBotRebateRetentionInfo> rrMessages, string newLogSIDs, string environment)
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
}
