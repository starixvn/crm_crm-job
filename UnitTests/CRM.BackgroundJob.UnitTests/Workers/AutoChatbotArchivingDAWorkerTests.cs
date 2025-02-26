namespace CRM.BackgroundJob.UnitTests.Workers
{
    using CRM.BackgroundJob.Models;
    using CRM.BackgroundJob.Workers;
    using CRM.Services;
    using Microsoft.Extensions.Options;
    using NSubstitute;
    using ONEbook.UM.Services;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CRM.Services.AutoChatBotMessage;
    using CRM.Services.Common;
    using Xunit;

    public class AutoChatbotArchivingDAWorkerTests
    {
        private IChatBotJobService chatBotJobService;
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
            await chatBotJobService.Received().PullMessageArchivingDAAndSendNotificationToHeadlineAsync();
        }

        private AutoChatbotArchivingDAWorker GetService()
        {
            appSettings =
            [
                new JobSetting
                {
                    JobName = "CreateFeedbackAmendmentNotiFB",
                    IsEnabled = true,
                    HasCheckUM = false,
                    TimeInterval = 60
                }
            ];
            var appJobSettings = Substitute.For<IOptions<List<JobSetting>>>();
            appJobSettings.Value.Returns(appSettings);
            chatBotJobService = Substitute.For<IChatBotJobService>();
            maintenanceService = Substitute.For<IMaintenanceService>();
            logService = Substitute.For<ICrmLogService>();

            return new AutoChatbotArchivingDAWorker(chatBotJobService, maintenanceService, appJobSettings, logService);
        }
    }
}