namespace CRM.BackgroundJob.Workers;

using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.BackgroundJob.Extensions;
using CRM.BackgroundJob.Models;
using CRM.Models.Common;
using CRM.Services.AutoChatBotMessage;
using CRM.Services.Common;
using Microsoft.Extensions.Options;
using ONEbook.UM.Services;

public class AutoChatbotProblemAccountWorker : BaseWorker
{
    private readonly IChatBotJobService chatBotJobService;

    public AutoChatbotProblemAccountWorker(
        IChatBotJobService chatBotJobService,
        IMaintenanceService maintenanceService,
        IOptions<List<JobSetting>> jobSettings,
        ICrmLogService logService)
        : base(maintenanceService, logService)
    {
        this.chatBotJobService = chatBotJobService;
        jobSetting = JobHelper.GetJobConfig(jobSettings.Value, JobName.PullMessageProblemAccountForChatBot);
    }

    public override async Task ExecuteJobAsync()
    {
        await chatBotJobService.PullMessageProblemsAccountAndSendNotificationToHeadlineAsync();
    }
}