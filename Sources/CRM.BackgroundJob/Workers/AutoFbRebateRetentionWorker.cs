namespace CRM.BackgroundJob.Workers;

using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.BackgroundJob.Extensions;
using CRM.BackgroundJob.Models;
using CRM.Models.Common;
using CRM.Services.AutoFeedback;
using CRM.Services.Common;
using Microsoft.Extensions.Options;
using ONEbook.UM.Services;

public class AutoFbRebateRetentionWorker : BaseWorker
{
    private readonly IAutoFeedbackService autoFeedbackService;

    public AutoFbRebateRetentionWorker(
        IAutoFeedbackService autoFeedbackService,
        IMaintenanceService maintenanceService,
        IOptions<List<JobSetting>> jobSettings,
        ICrmLogService logService)
        : base(maintenanceService, logService)
    {
        this.autoFeedbackService = autoFeedbackService;
        jobSetting = JobHelper.GetJobConfig(jobSettings.Value, JobName.CreateFeedbackForRebateRetention);
    }

    public override async Task ExecuteJobAsync()
    {
        await autoFeedbackService.RebateRetentionExecuteJobAsync();
    }
}