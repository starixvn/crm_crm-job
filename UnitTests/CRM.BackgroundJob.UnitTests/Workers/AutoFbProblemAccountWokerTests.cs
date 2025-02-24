namespace CRM.BackgroundJob.UnitTests.Workers
{
    using CRM.BackgroundJob.Models;
    using CRM.BackgroundJob.Services;
    using CRM.BackgroundJob.Workers;
    using CRM.Services;
    using Microsoft.Extensions.Options;
    using NSubstitute;
    using ONEbook.UM.Services;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;

    public class AutoFbProblemAccountWokerTests
    {
        private IAutoFeedbackService autoFeedbackService;
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
            await autoFeedbackService.Received().ProblemsAccountExecuteJobAsync();
        }

        private AutoFbProblemAccountWorker GetService()
        {
            appSettings =
            [
                new JobSetting
                {
                    JobName = "PullMessageCTOForChatBot",
                    IsEnabled = true,
                    HasCheckUM = false,
                    TimeInterval = 60
                }
            ];
            var appJobSettings = Substitute.For<IOptions<List<JobSetting>>>();
            appJobSettings.Value.Returns(appSettings);
            autoFeedbackService = Substitute.For<IAutoFeedbackService>();
            maintenanceService = Substitute.For<IMaintenanceService>();
            logService = Substitute.For<ICrmLogService>();

            return new AutoFbProblemAccountWorker(autoFeedbackService, maintenanceService, appJobSettings, logService);
        }
    }
}