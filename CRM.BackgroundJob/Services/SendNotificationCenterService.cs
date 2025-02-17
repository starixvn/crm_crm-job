namespace CRM.BackgroundJob.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CRM.BackgroundJob.Criteria.SendNotificationCenter;
    using CRM.BackgroundJob.Models.MessageNotificationCenter;
    using CRM.BackgroundJob.Core.Models;
    using CRM.BackgroundJob.Core.Services;

    public interface ISendNotificationCenterService
    {
        Task<CommandResult> InsertNotificationAlertAsync(string environment);

        Task<IEnumerable<MessageBotInfo>> GetNotificationAlertAsync(string environment);
    }

    public class SendNotificationCenterService : ISendNotificationCenterService
    {
        private readonly IBaseService baseService;

        public SendNotificationCenterService(IBaseService baseService)
        {
            this.baseService = baseService;
        }

        public async Task<CommandResult> InsertNotificationAlertAsync(string environment) {
            var command = new InsertNotificationAlertCommand
            {
                Environment = environment
            };
            return await baseService.ExecuteNonQueryAsync(command);
        }

        public async Task<IEnumerable<MessageBotInfo>> GetNotificationAlertAsync(string environment)
        {
            var criteria = new GetNotificationAlerCriteria
            {
                Environment = environment
            };
            return await baseService.GetDataAsync<MessageBotInfo>(criteria);
        }
    }
}
