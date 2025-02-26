namespace CRM.BackgroundJob.Workers;

using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.BackgroundJob.Extensions;
using CRM.BackgroundJob.Models;
using CRM.Models.Common;
using CRM.Services;
using CRM.Services.Common;
using Microsoft.Extensions.Options;
using ONEbook.UM.Services;
using Services.NotificationCenter;

public class MncGenerateLapNotificationWorker : BaseWorker
{
    private readonly IMessageNotificationCenterService messageNotificationCenter;

    public MncGenerateLapNotificationWorker(
        IMessageNotificationCenterService messageNotificationCenter,
        IMaintenanceService maintenanceService,
        IOptions<List<JobSetting>> jobSettings,
        ICrmLogService logService)
        : base(maintenanceService, logService)
    {
        this.messageNotificationCenter = messageNotificationCenter;
        jobSetting = JobHelper.GetJobConfig(jobSettings.Value, JobName.GenerateLapNotification);
    }

    public override async Task ExecuteJobAsync()
    {
        await messageNotificationCenter.GenerateLapNotificationAsync();
    }
}