namespace CRM.BackgroundJob.UnitTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using CRM.Models;
    using CRM.Services;
    using CRM.BackgroundJob.Criteria.AbnormalBet;
    using CRM.BackgroundJob.Models;
    using CRM.BackgroundJob.Models.AbnormalBet;
    using CRM.BackgroundJob.Services;
    using NSubstitute;
    using Xunit;

    public class AbnormalBetServiceTests
    {
        private IBaseService baseService;

        [Fact]
        public async Task GetVoidedInfoAsync_WhenCalled_ReturnExpected()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture.Create<ValueTuple<IEnumerable<VoidedInfo>, IEnumerable<VoidingMessage>>>();
            var service = GetService();
            baseService.FetchMultipleAsync<VoidedInfo, VoidingMessage>(Arg.Any<GetAbnormalBetVoidedInfoCriteria>()).Returns(expected);

            // Act
            var actual = await service.GetVoidedInfoAsync(1, 2);

            // Assert
            Assert.Equal(expected.Item1, actual.Item1);
            Assert.Equal(expected.Item2, actual.Item2);
        }

        [Fact]
        public async Task GetMessageAbnormalBetAsync_WhenCalled_ReturnExpected()
        {
            // Arrange
            var fixture = new Fixture();
            var message = fixture.Create<IEnumerable<VoidingMessage>>();
            var expected = fixture.Create<IEnumerable<MessageAbnormelBet>>();
            var service = GetService();
            baseService.GetDataAsync<MessageAbnormelBet>(Arg.Any<GetAbnormalBetMessageCriteria>()).Returns(expected);

            // Act
            var actual = await service.GetMessageAbnormalBetAsync(message);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ImportAbnormalBetsAsync_WhenCalled_ReturnExpected()
        {
            // Arrange
            var fixture = new Fixture();
            var message = fixture.Create<IEnumerable<MessageAbnormelBet>>();
            var voidedCollectionInfo = fixture.Create<IEnumerable<VoidedInfo>>();
            var commandResult = fixture.Create<CommandResult>();
            var command = fixture.Create<ImportAbnormalBetCommand>();
            var expected = new ValueTuple<CommandResult, ImportAbnormalBetCommand>(commandResult, command);
            var service = GetService();
            baseService.ExecuteNonQueryAsync(Arg.Any<ImportAbnormalBetCommand>()).Returns(commandResult);

            // Act
            var actual = await service.ImportAbnormalBetsAsync(message, voidedCollectionInfo, "UAT", 1);

            // Assert
            Assert.Equal(expected.Item1, actual.Item1);
        }

        #region Helpers
        private AbnormalBetService GetService()
        {
            baseService = Substitute.For<IBaseService>();
            return new AbnormalBetService(baseService);
        }
        #endregion Helpers
    }
}
