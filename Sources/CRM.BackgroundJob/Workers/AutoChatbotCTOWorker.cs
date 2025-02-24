namespace CRM.BackgroundJob.Workers;

using Constants;
using CRM.BackgroundJob.Extensions;
using CRM.BackgroundJob.Models;
using CRM.Services;
using Microsoft.Extensions.Options;
using ONEbook.UM.Services;
using Services;
using System.Collections.Generic;
using System.Threading.Tasks;

public class AutoChatbotCTOWorker : BaseWorker
{
    private readonly IChatBotJobService chatBotJobService;

    public AutoChatbotCTOWorker(
        IChatBotJobService chatBotJobService,
        IMaintenanceService maintenanceService,
        IOptions<List<JobSetting>> jobSettings,
        ICrmLogService logService)
        : base(maintenanceService, logService)
    {
        this.chatBotJobService = chatBotJobService;
        jobSetting = JobHelper.GetJobConfig(jobSettings.Value, JobName.PullMessageCTOForChatBot);
    }

    public override async Task ExecuteJobAsync()
    {
        await chatBotJobService.PullMessageCTOAndSendNotificationToHeadlineAsync();
    }
}