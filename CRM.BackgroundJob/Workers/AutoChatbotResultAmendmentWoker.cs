namespace CRM.BackgroundJob.Workers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CRM.BackgroundJob.Extensions;
    using CRM.BackgroundJob.Models;
    using CRM.BackgroundJob.Core.Services;
    using Fanex.CRM.BackgroundJob.Constants;
    using Fanex.CRM.BackgroundJob.Services;
    using Microsoft.Extensions.Options;
    using ONEbook.UM.Services;

    public class AutoChatbotResultAmendmentWoker : BaseWorker
    {
        private readonly IChatBotJobService chatBotJobService;

        public AutoChatbotResultAmendmentWoker(
            IChatBotJobService chatBotJobService,
            IMaintenanceService maintenanceService,
            IOptions<List<JobSetting>> jobSettings,
            ICrmLogService logService)
                : base(maintenanceService, logService)
        {
            this.chatBotJobService = chatBotJobService;
            jobSetting = JobHelper.GetJobConfig(jobSettings.Value, JobName.PullMessageResultAmendmentForChatBot);
        }

        public override async Task ExecuteJobAsync()
        {
            await chatBotJobService.PullMessageResultAmendmentAndSendNotificationToHeadlineAsync();
        }
    }
}