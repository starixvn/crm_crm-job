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

    public class ScanAndSendLetstalkNotificationWorkerTests
    {
        private ISendNotificationService sendNotificationService;
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
            await sendNotificationService.Received().ScanAndSendMessageToBotEndpointAsync();
        }

        private ScanAndSendLetstalkNotificationWorker GetService()
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
            sendNotificationService = Substitute.For<ISendNotificationService>();
            maintenanceService = Substitute.For<IMaintenanceService>();
            logService = Substitute.For<ICrmLogService>();

            return new ScanAndSendLetstalkNotificationWorker(sendNotificationService, maintenanceService, appJobSettings, logService);
        }
    }
}