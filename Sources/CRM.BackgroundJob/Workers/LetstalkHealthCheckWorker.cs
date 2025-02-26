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

public class LetstalkHealthCheckWorker : BaseWorker
{
    private readonly IBotHealthCheckService botHealthCheckService;

    public LetstalkHealthCheckWorker(
        IBotHealthCheckService botHealthCheckService,
        IMaintenanceService maintenanceService,
        IOptions<List<JobSetting>> jobSettings,
        ICrmLogService logService)
        : base(maintenanceService, logService)
    {
        this.botHealthCheckService = botHealthCheckService;
        jobSetting = JobHelper.GetJobConfig(jobSettings.Value, JobName.MonitorLetstalkService);
    }

    public override async Task ExecuteJobAsync()
    {
        await botHealthCheckService.LetstalkBotMonitorAsync();
    }
}