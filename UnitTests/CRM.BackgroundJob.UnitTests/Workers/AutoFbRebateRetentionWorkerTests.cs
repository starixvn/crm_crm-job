namespace CRM.BackgroundJob.UnitTests.Workers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CRM.Services;
    using CRM.BackgroundJob.Models;
    using CRM.BackgroundJob.Workers;
    using CRM.Services.AutoFeedback;
    using CRM.Services.Common;
    using Microsoft.Extensions.Options;
    using NSubstitute;
    using ONEbook.UM.Services;
    using Xunit;

    public class AutoFbRebateRetentionWorkerTests
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
            await autoFeedbackService.Received().RebateRetentionExecuteJobAsync();
        }
        private AutoFbRebateRetentionWorker GetService()
        {
            appSettings =
            [
                new JobSetting
                {
                    JobName = "CreateFeedbackForRebateRetention",
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

            return new AutoFbRebateRetentionWorker(autoFeedbackService, maintenanceService, appJobSettings, logService);
        }
    }
}
