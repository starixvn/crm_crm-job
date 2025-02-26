namespace CRM.BackgroundJob.Workers;

using CRM.BackgroundJob.Extensions;
using CRM.BackgroundJob.Models;
using CRM.Services;
using Microsoft.Extensions.Options;
using ONEbook.UM.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Models.Common;
using CRM.Services.AutoFeedback;
using CRM.Services.Common;

public class AutoFbProblemAccountWorker : BaseWorker
{
    private readonly IAutoFeedbackService autoFeedbackService;

    public AutoFbProblemAccountWorker(
        IAutoFeedbackService autoFeedbackService,
        IMaintenanceService maintenanceService,
        IOptions<List<JobSetting>> jobSettings,
        ICrmLogService logService)
        : base(maintenanceService, logService)
    {
        this.autoFeedbackService = autoFeedbackService;
        jobSetting = JobHelper.GetJobConfig(jobSettings.Value, JobName.CreateFeedbackForProblemsAccount);
    }

    public override async Task ExecuteJobAsync()
    {
        await autoFeedbackService.ProblemsAccountExecuteJobAsync();
    }
}