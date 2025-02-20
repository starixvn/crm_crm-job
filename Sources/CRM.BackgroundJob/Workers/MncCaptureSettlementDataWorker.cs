namespace CRM.BackgroundJob.Workers;

using System.Collections.Generic;
using System.Threading.Tasks;
using Constants;
using CRM.BackgroundJob.Extensions;
using CRM.BackgroundJob.Models;
using CRM.BackgroundJob.Services;
using CRM.Services;
using Microsoft.Extensions.Options;
using ONEbook.UM.Services;

public class MncCaptureSettlementDataWorker : BaseWorker
{
    private readonly IMessageNotificationCenterService messageNotificationCenter;

    public MncCaptureSettlementDataWorker(
        IMessageNotificationCenterService messageNotificationCenter,
        IMaintenanceService maintenanceService,
        IOptions<List<JobSetting>> jobSettings,
        ICrmLogService logService)
        : base(maintenanceService, logService)
    {
        this.messageNotificationCenter = messageNotificationCenter;
        jobSetting = JobHelper.GetJobConfig(jobSettings.Value, JobName.CaptureSettlementData);
    }

    public override async Task ExecuteJobAsync()
    {
        await messageNotificationCenter.CaptureSettlementDataAsync();
    }
}