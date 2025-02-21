namespace CRM.BackgroundJob.UnitTests.Workers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CRM.Services;
    using CRM.BackgroundJob.Models;
    using CRM.BackgroundJob.Workers;
    using Microsoft.Extensions.Options;
    using NSubstitute;
    using ONEbook.UM.Services;
    using Xunit;

    public class ScanAndSendAnnouncementWorkerTests
    {
        private ISendAnnouncementService sendAnnouncementService;
        private IMaintenanceService maintenanceService;
        private ICrmLogService logService;
        private List<JobSetting> appSettings;

        [Fact]
        public async Task ExecuteJobAsync_Override_CallExactFunction()
        {
            //Arrange
            var service = GetService();

            // Act
            await service.ExecuteJobAsync();

            // Assert
        }

        private ScanAndSendAnnouncementWorker GetService()
        {
            appSettings = new List<JobSetting>
            {
                new JobSetting
                {
                   JobName = "Test",
                   IsEnabled = true,
                   HasCheckUM = false,
                   TimeInterval = 60
                }
            };
            var appJobSettings = Substitute.For<IOptions<List<JobSetting>>>();
            appJobSettings.Value.Returns(appSettings);
            sendAnnouncementService = Substitute.For<ISendAnnouncementService>();
            maintenanceService = Substitute.For<IMaintenanceService>();
            logService = Substitute.For<ICrmLogService>();

            return new ScanAndSendAnnouncementWorker(sendAnnouncementService, maintenanceService, appJobSettings, logService);
        }
    }
}