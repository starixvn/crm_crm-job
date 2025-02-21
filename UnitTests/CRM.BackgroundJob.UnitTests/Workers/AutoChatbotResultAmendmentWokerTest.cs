
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

    public class AutoChatbotResultAmendmentWokerTests
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
            await chatBotJobService.Received().PullMessageResultAmendmentAndSendNotificationToHeadlineAsync();
        }
        private AutoChatbotResultAmendmentWoker GetService()
        {
            appSettings = new List<JobSetting>
            {
                new JobSetting
                {
                   JobName = "PullMessageResultAmendmentForChatBot",
                   IsEnabled = true,
                   HasCheckUM = false,
                   TimeInterval = 60
                }
            };
            var appJobSettings = Substitute.For<IOptions<List<JobSetting>>>();
            appJobSettings.Value.Returns(appSettings);
            chatBotJobService = Substitute.For<IChatBotJobService>();
            maintenanceService = Substitute.For<IMaintenanceService>();
            logService = Substitute.For<ICrmLogService>();

            return new AutoChatbotResultAmendmentWoker(chatBotJobService, maintenanceService, appJobSettings, logService);
        }
    }
}
