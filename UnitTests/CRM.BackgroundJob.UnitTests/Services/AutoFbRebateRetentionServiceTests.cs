namespace CRM.BackgroundJob.UnitTests.Services
{
    using AutoFixture;
    using CRM.BackgroundJob.Criteria.AutoFbRebateRetention;
    using CRM.BackgroundJob.Criteria.ChatBotRebateRetention;
    using CRM.BackgroundJob.Models.ChatBotRebateRetention;
    using CRM.BackgroundJob.Services;
    using CRM.Models;
    using CRM.Services;
    using NSubstitute;
    using System.Collections.Generic;
    using System.Threading.Tasks;
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
            var service = GetService();
            baseService.ExecuteNonQueryAsync(Arg.Do<ImportAutoFbRebateRetentionCommand>(x => _ = x)).Returns(commandResult);

            // Act
            var actual = await service.ImportRebateRetentionCommand(rrMessages, "1", "dev");

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