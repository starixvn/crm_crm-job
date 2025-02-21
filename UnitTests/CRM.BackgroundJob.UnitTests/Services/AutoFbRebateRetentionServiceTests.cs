namespace CRM.BackgroundJob.UnitTests.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using CRM.Models;
    using CRM.Services;
    using CRM.BackgroundJob.Criteria.AutoFbRebateRetention;
    using CRM.BackgroundJob.Criteria.ChatBotRebateRetention;
    using CRM.BackgroundJob.Models.ChatBotRebateRetention;
    using CRM.BackgroundJob.Services;
    using NSubstitute;
    using Xunit;

    public class AutoFbRebateRetentionServiceTests
    {
        private IBaseService baseService;

        [Fact]
        public async Task GetRebateRetentionInfoAsync_WhenCalled_ReturnExpected()
        {
            // Arrange
            var fixture = new Fixture();
            var rrInfos = fixture.Create<IEnumerable<ChatBotRebateRetentionInfo>>();
            var criteria = new GetRebateRetentionInfoCriteria();
            var expected = (rrInfos, criteria.NewLogSID);
            var service = GetService();
            baseService.GetDataAsync<ChatBotRebateRetentionInfo>(Arg.Do<GetRebateRetentionInfoCriteria>(x => criteria = x)).Returns(rrInfos);

            // Act
            var actual = await service.GetRebateRetentionInfoAsync("1");

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ImportRebateRetentionCommand_WhenCalled_ReturnExpected()
        {
            // Arrange
            var fixture = new Fixture();
            var rrMessages = fixture.Create<IEnumerable<ChatBotRebateRetentionInfo>>();
            var commandResult = fixture.Create<CommandResult>();
            var command = new ImportAutoFbRebateRetentionCommand();
            var service = GetService();
            baseService.ExecuteNonQueryAsync(Arg.Do<ImportAutoFbRebateRetentionCommand>(x => command = x)).Returns(commandResult);

            // Act
            var actual = await service.ImportRebateRetentionCommand(rrMessages, "1" ,"dev");

            // Assert
            Assert.Equal(commandResult, actual.Item1);
        }

        #region Helpers
        private AutoFbRebateRetentionService GetService()
        {
            baseService = Substitute.For<IBaseService>();
            return new AutoFbRebateRetentionService(baseService);
        }
        #endregion Helpers
    }
}
