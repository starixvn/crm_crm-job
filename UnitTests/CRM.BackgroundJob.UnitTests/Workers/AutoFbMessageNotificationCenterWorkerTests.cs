namespace CRM.BackgroundJob.UnitTests.Workers
{
    using CRM.BackgroundJob.Models;
    using CRM.BackgroundJob.Workers;
    using Microsoft.Extensions.Options;
    using NSubstitute;
    using ONEbook.UM.Services;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CRM.Services.AutoFeedback;
    using CRM.Services.Common;
    using Xunit;

    public class AutoFbMessageNotificationCenterWorkerTests
    {
        private IAutoFeedbackService autoFeedbackService;
        private IMaintenanceService maintenanceService;
        private List<JobSetting> appSettings;
        private ICrmLogService logService;

        [Fact]
        public async Task ExecuteJobAsync_Override_CallExactFunction()
        {
            //Arrange
            var service = GetService();

            // Act
            await service.ExecuteJobAsync();

            // Assert
            await autoFeedbackService.Received().MessageNotificationCenterExecuteJobAsync();
        }

        private AutoFbMessageNotificationCenterWorker GetService()
        {
            appSettings =
            [
                new JobSetting
                {
                    JobName = "Test",
                    IsEnabled = true,
                    HasCheckUM = false,
                    IsDisableAtWeekend = false,
                    TimeInterval = 60
                }
            ];
            var appJobSettings = Substitute.For<IOptions<List<JobSetting>>>();
            appJobSettings.Value.Returns(appSettings);
            autoFeedbackService = Substitute.For<IAutoFeedbackService>();
            maintenanceService = Substitute.For<IMaintenanceService>();
            logService = Substitute.For<ICrmLogService>();

            return new AutoFbMessageNotificationCenterWorker(autoFeedbackService, maintenanceService, appJobSettings, logService);
        }
    }
}