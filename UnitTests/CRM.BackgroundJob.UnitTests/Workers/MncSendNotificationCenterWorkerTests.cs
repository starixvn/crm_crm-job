namespace CRM.BackgroundJob.UnitTests.Workers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CRM.BackgroundJob.Models;
    using CRM.BackgroundJob.Workers;
    using CRM.Services.Common;
    using CRM.Services.NotificationCenter;
    using Microsoft.Extensions.Options;
    using NSubstitute;
    using ONEbook.UM.Services;
    using Xunit;

    public class MncSendNotificationCenterWorkerTests
    {
        private IMessageNotificationCenterService messageNotificationCenter;
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
            await messageNotificationCenter.Received().SendNotificationCenterAsync();
        }

        private MncSendNotificationCenterWorker GetService()
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
            messageNotificationCenter = Substitute.For<IMessageNotificationCenterService>();
            maintenanceService = Substitute.For<IMaintenanceService>();
            logService = Substitute.For<ICrmLogService>();

            return new MncSendNotificationCenterWorker(messageNotificationCenter, maintenanceService, appJobSettings, logService);
        }
    }
}
