namespace CRM.BackgroundJob.UnitTests.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using CRM.BackgroundJob.Core.Models;
    using CRM.BackgroundJob.Core.Services;
    using CRM.BackgroundJob.Criteria.AutoFbMessageNotificationCenter;
    using CRM.BackgroundJob.Models;
    using CRM.BackgroundJob.Services;
    using NSubstitute;
    using Xunit;

    public class AutoFbMessageNotificationCenterServiceTests
    {
        private IBaseService baseService;

        [Fact]
        public async Task GetMsgInfoMessageNotificationAsync_WhenCalled_ReturnExpected()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture.Create<IEnumerable<MessageInfoID>>();
            var service = GetService();
            baseService.GetDataAsync<MessageInfoID>(Arg.Any<GetMsgInfoMessageNotificationCriteria>()).Returns(expected);

            // Act
            var actual = await service.GetMsgInfoMessageNotificationAsync(1, 2);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ImportMessageNotificationAsync_WhenCalled_ReturnExpected()
        {
            // Arrange
            var fixture = new Fixture();
            var messages = fixture.Create<IEnumerable<MessageInfoID>>();
            var commandResult = fixture.Create<CommandResult>();
            var command = new ImportMessageNotificationCenterCommand();
            var service = GetService();
            baseService.ExecuteNonQueryAsync(Arg.Do<ImportMessageNotificationCenterCommand>(x => command = x)).Returns(commandResult);

            // Act
            var actual = await service.ImportMessageNotificationAsync(messages, "dev");

            // Assert
            Assert.Equal(commandResult, actual.commandResult);
        }

        #region Helpers
        private AutoFbMessageNotificationCenterService GetService()
        {
            baseService = Substitute.For<IBaseService>();
            return new AutoFbMessageNotificationCenterService(baseService);
        }
        #endregion Helpers
    }
}
