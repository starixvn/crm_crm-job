namespace CRM.BackgroundJob.Services
{
    using System.Threading.Tasks;
    using CRM.BackgroundJob.Models.HealthCheck;
    using CRM.BackgroundJob.Core.Enumerables;
    using CRM.BackgroundJob.Core.Models.BotServices;
    using CRM.BackgroundJob.Core.Services;
    using Newtonsoft.Json;

    public interface IBotHealthCheckService
    {
        Task LetstalkBotMonitorAsync();
    }

    public class BotHealthCheckService : IBotHealthCheckService
    {
        private readonly IChatBotSendMessageService chatBotSendMessageService;
        private readonly LetstalkHealthCheckSettings healthCheckSettings;
        private readonly ICrmLogService logService;

        public BotHealthCheckService(IChatBotSendMessageService chatBotSendMessageService,
            LetstalkHealthCheckSettings healthCheckSettings,
            ICrmLogService logService)
        {
            this.chatBotSendMessageService = chatBotSendMessageService;
            this.healthCheckSettings = healthCheckSettings;
            this.logService = logService;
        }

        public async Task LetstalkBotMonitorAsync()
        {
            var notificationMsg = new NotificationMessage
            {
                BotId = healthCheckSettings.BotId,
                ChannelId = BotChannel.Letstalk,
                ChatTypeId = (int)ChatType.Personal,
                ConversationId = healthCheckSettings.LetstalkId,
                Content = healthCheckSettings.Message
            };

            var sentResult = await chatBotSendMessageService.SendSinglePlaintexMessageAsync(BotChannel.Letstalk, notificationMsg);
            if (sentResult is not null && !sentResult.IsSuccessful)
            {
                await logService.InfoMessageAsync(JsonConvert.SerializeObject(sentResult));
                await logService.ErrorMessageAsync($"Letstalk service unhealthy, the result error: {sentResult.ErrorMessage}");
            }
        }
    }
}