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

public class AutoFbRefundSettledTicketWorker : BaseWorker
{
    private readonly IAutoFeedbackService autoFeedbackService;

    public AutoFbRefundSettledTicketWorker(
        IAutoFeedbackService autoFeedbackService,
        IMaintenanceService maintenanceService,
        IOptions<List<JobSetting>> jobSettings,
        ICrmLogService logService)
        : base(maintenanceService, logService)
    {
        this.autoFeedbackService = autoFeedbackService;
        jobSetting = JobHelper.GetJobConfig(jobSettings.Value, JobName.CreateFeedbackForRefundSettledTicket);
    }

    public override async Task ExecuteJobAsync()
    {
        await autoFeedbackService.RefundSettledTicketJobAsync();
    }
}