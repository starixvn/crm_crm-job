namespace CRM.BackgroundJob.Workers;

using System.Collections.Generic;
using System.Threading.Tasks;
using Constants;
using CRM.BackgroundJob.Extensions;
using CRM.BackgroundJob.Models;
using CRM.Services;
using Microsoft.Extensions.Options;
using ONEbook.UM.Services;
using Services;

public class AutoFbProblemAccountWoker : BaseWorker
{
    private readonly IAutoFeedbackService autoFeedbackService;

    public AutoFbProblemAccountWoker(
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
        await autoFeedbackService.ProblemsAcccountExecuteJobAsync();
    }
}