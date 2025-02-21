namespace CRM.BackgroundJob.Workers;

using System.Collections.Generic;
using System.Threading.Tasks;
using Constants;
using CRM.BackgroundJob.Extensions;
using CRM.BackgroundJob.Models;
using CRM.Services;
using Microsoft.Extensions.Options;
using ONEbook.UM.Services;

public class ScanAndSendAnnouncementWorker : BaseWorker
{
    private readonly ISendAnnouncementService sendService;

    public ScanAndSendAnnouncementWorker(
        ISendAnnouncementService sendService,
        IMaintenanceService maintenanceService,
        IOptions<List<JobSetting>> jobSettings,
        ICrmLogService logService)
        : base(maintenanceService, logService)
    {
        this.sendService = sendService;
        jobSetting = JobHelper.GetJobConfig(jobSettings.Value, JobName.ScanAndSendAnnouncement);
    }

    public override async Task ExecuteJobAsync()
    {
        await sendService.ScanSendAndUpdateAnnouncementNotification();
    }
}