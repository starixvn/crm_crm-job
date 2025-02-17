namespace CRM.BackgroundJob.UnitTests.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using CRM.BackgroundJob.Core.Extensions.Xml;
    using CRM.BackgroundJob.Core.Models;
    using CRM.BackgroundJob.Core.Services;
    using CRM.BackgroundJob.Criteria.ArchivingDA;
    using CRM.BackgroundJob.Criteria.TurnoverControll;
    using CRM.BackgroundJob.Criteria.WinLossLimit;
    using CRM.BackgroundJob.Models.AbnormalBet;
    using CRM.BackgroundJob.Models.WinLossLimit;
    using Fanex.CRM.BackgroundJob.Constants;
    using Fanex.CRM.BackgroundJob.Criteria;
    using Fanex.CRM.BackgroundJob.Criteria.ChatBot;
    using Fanex.CRM.BackgroundJob.Models;
    using Fanex.CRM.BackgroundJob.Services;
    using NSubstitute;
    using Xunit;

    public class SystemSettingServiceTests
    {
        private IBaseService baseService;

        [Fact]
        public async Task GetAbnormalBetSettingAsync_WhenCalled_ReturnExpectedSetting()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture.Create<AbnormalBetSystemSetting>();
            var service = GetSystemSettingService();
            baseService.GetDataEntityAsync<AbnormalBetSystemSetting>(Arg.Any<GetSystemSettingByServiceNameCriteria>()).Returns(expected);

            // Act
            var actual = await service.GetAbnormalBetSettingAsync(BackgroundJobConstants.ServiceName_AutoFB);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task EditSystemSettingAsync_WhenCalled_ReturnExpectedCommandResult()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture.Create<CommandResult>();
            var settings = fixture.Create<IEnumerable<SystemSettingItem>>();
            var xml = settings.ToXML("ROOT").ToString();
            var criteria = new EditSystemSettingCommand();
            var service = GetSystemSettingService();
            baseService.ExecuteNonQueryAsync(Arg.Do<EditSystemSettingCommand>(x => criteria = x)).Returns(expected);

            // Act
            var actual = await service.EditSystemSettingAsync(settings);

            // Assert
            Assert.Equal(expected, actual);
            Assert.Equal(xml, criteria.Settings);
            Assert.Equal(BackgroundJobConstants.ServiceName_AutoFB, criteria.ServiceName);
        }

        [Fact]
        public async Task EditJobStatusAsync_WhenCalled_ReturnExpectedCommandResult()
        {
            // Arrange
            var fixture = new Fixture();
            var status = fixture.Create<string>();
            var expected = fixture.Create<CommandResult>();
            var settings = new List<SystemSettingItem>
            {
                new SystemSettingItem { SettingName = SystemSettingName.JobStatus, Value = status }
            };
            var xml = settings.ToXML("ROOT").ToString();
            var criteria = new EditSystemSettingCommand();
            var service = GetSystemSettingService();
            baseService.ExecuteNonQueryAsync(Arg.Do<EditSystemSettingCommand>(x => criteria = x)).Returns(expected);

            // Act
            var actual = await service.EditJobStatusAsync(status);

            // Assert
            Assert.Equal(expected, actual);
            Assert.Equal(xml, criteria.Settings);
        }

        [Fact]
        public async Task GetChatBotSystemSettingAsync_WhenCalled_ReturnExpectedSetting()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture.Create<AbnormalBetSystemSetting>();
            var criteria = new GetChatBotSystemSettingCriteria();
            var service = GetSystemSettingService();
            baseService.GetDataEntityAsync<AbnormalBetSystemSetting>(Arg.Do<GetChatBotSystemSettingCriteria>(x => criteria = x)).Returns(expected);

            // Act
            var actual = await service.GetChatBotSystemSettingAsync(BackgroundJobConstants.ServiceName_ChatBot, BackgroundJobConstants.DefaultEnvironment);

            // Assert
            Assert.Equal(expected, actual);
            Assert.Equal(BackgroundJobConstants.DefaultEnvironment, criteria.Environment);
            Assert.Equal(BackgroundJobConstants.ServiceName_ChatBot, criteria.ServiceName);
        }

        [Fact]
        public async Task GetWinLossSettingAsync_WhenCalled_ReturnExpectedSetting()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture.Create<WinLossSystemSetting>();
            var service = GetSystemSettingService();
            baseService.GetDataEntityAsync<WinLossSystemSetting>(Arg.Any<GetWinLossSystemSettingCriteria>()).Returns(expected);

            // Act
            var actual = await service.GetWinLossSettingAsync();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task GetTurnoverSettingAsync_WhenCalled_ReturnExpectedSetting()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture.Create<WinLossSystemSetting>();
            var service = GetSystemSettingService();
            baseService.GetDataEntityAsync<WinLossSystemSetting>(Arg.Any<GetTOControllSystemSettingCriteria>()).Returns(expected);

            // Act
            var actual = await service.GetTurnoverSettingAsync();

            // Assert
            Assert.Equal(expected, actual);
        }

        #region Helpers

        private SystemSettingService GetSystemSettingService()
        {
            baseService = Substitute.For<IBaseService>();
            return new SystemSettingService(baseService);
        }

        #endregion Helpers
    }
}