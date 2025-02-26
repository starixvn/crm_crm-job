namespace CRM.BackgroundJob.Workers;

using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.BackgroundJob.Extensions;
using CRM.BackgroundJob.Models;
using CRM.Models.Common;
using CRM.Services;
using CRM.Services.AutoChatBotMessage;
using CRM.Services.Common;
using Microsoft.Extensions.Options;
using ONEbook.UM.Services;

public class AutoChatbotWorker : BaseWorker
{
    private readonly IChatBotJobService chatBotJobService;

    public AutoChatbotWorker(
        IChatBotJobService chatBotJobService,
        IMaintenanceService maintenanceService,
        IOptions<List<JobSetting>> jobSettings,
        ICrmLogService logService)
        : base(maintenanceService, logService)
    {
        this.chatBotJobService = chatBotJobService;
        jobSetting = JobHelper.GetJobConfig(jobSettings.Value, JobName.PullMessageForChatBot);
    }

    public override async Task ExecuteJobAsync()
    {
        await chatBotJobService.PullMessageAndSendNotificationAsync();
    }
}