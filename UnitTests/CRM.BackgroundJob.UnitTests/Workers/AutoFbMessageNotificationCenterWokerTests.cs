namespace CRM.BackgroundJob.UnitTests.Workers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CRM.Services;
    using CRM.BackgroundJob.Models;
    using CRM.BackgroundJob.Workers;
    using CRM.BackgroundJob.Services;
    using Microsoft.Extensions.Options;
    using NSubstitute;
    using ONEbook.UM.Services;
    using Xunit;

    public class AutoFbMessageNotificationCenterWokerTests
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

        private AutoFbMessageNotificationCenterWoker GetService()
        {
            appSettings = new List<JobSetting>
            {
                new JobSetting
                {
                   JobName = "Test",
                   IsEnabled = true,
                   HasCheckUM = false,
                   IsDisableAtWeekend = false,
                   TimeInterval = 60
                }
            };
            var appJobSettings = Substitute.For<IOptions<List<JobSetting>>>();
            appJobSettings.Value.Returns(appSettings);
            autoFeedbackService = Substitute.For<IAutoFeedbackService>();
            maintenanceService = Substitute.For<IMaintenanceService>();
            logService = Substitute.For<ICrmLogService>();

            return new AutoFbMessageNotificationCenterWoker(autoFeedbackService, maintenanceService, appJobSettings, logService);
        }
    }
}
