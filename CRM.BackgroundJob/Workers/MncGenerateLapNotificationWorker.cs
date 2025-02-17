namespace CRM.BackgroundJob.Workers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CRM.BackgroundJob.Extensions;
    using CRM.BackgroundJob.Models;
    using CRM.BackgroundJob.Services;
    using CRM.BackgroundJob.Core.Services;
    using Fanex.CRM.BackgroundJob.Constants;
    using Microsoft.Extensions.Options;
    using ONEbook.UM.Services;

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
}
