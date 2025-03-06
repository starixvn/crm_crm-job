namespace CRM.UnitTests.Services.AutoFeedback
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using CRM.Constants;
    using CRM.Criteria.ArchivingDA;
    using CRM.Criteria.AutoFeedback;
    using CRM.Criteria.ChatBot;
    using CRM.Criteria.TurnoverControl;
    using CRM.Criteria.WinLossLimit;
    using CRM.Extensions.Xml;
    using CRM.Models.AbnormalBet;
    using CRM.Models.Common;
    using CRM.Models.WinLossLimit;
    using CRM.Services.AutoFeedback;
    using CRM.Services.Common;
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
            var expected = fixture.Create<CRM.Models.Common.CommandResult>();
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
            var expected = fixture.Create<CRM.Models.Common.CommandResult>();
            var settings = new List<SystemSettingItem>
            {
                new() { SettingName = SystemSettingName.JobStatus, Value = status }
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
            var actual = await service.GetChatBotSystemSettingAsync(BackgroundJobConstants.ServiceName_ChatBot);

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
            baseService.GetDataEntityAsync<WinLossSystemSetting>(Arg.Any<GetTOControlSystemSettingCriteria>()).Returns(expected);

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