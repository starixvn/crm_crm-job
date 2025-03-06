namespace CRM.UnitTests.Services.AutoFeedback
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using CRM.Criteria.VoidingBetID;
    using CRM.Models.Common;
    using CRM.Services.AutoFeedback;
    using CRM.Services.Common;
    using NSubstitute;
    using Xunit;

    public class VoidingBetIDServiceTests
    {
        private IBaseService baseService;

        [Fact]
        public async Task GetMessageVoidBetIDAsync_WhenCalled_ReturnExpected()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture.Create<IEnumerable<MessageInfoID>>();
            var service = GetService();
            baseService.GetDataAsync<MessageInfoID>(Arg.Any<GetMessageVoidByBetIDCriteria>()).Returns(expected);

            // Act
            var actual = await service.GetMessageVoidBetIDAsync(1, 2);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task CleanMessageVoidBetIDAsync_WhenCalled_ReturnExpected()
        {
            // Arrange
            var fixture = new Fixture();
            var message = fixture.Create<IEnumerable<MessageInfoID>>();
            var expected = fixture.Create<IEnumerable<MessageInfoID>>();
            var service = GetService();
            baseService.GetDataAsync<MessageInfoID>(Arg.Any<CleanMessageVoidByBetIDCriteria>()).Returns(expected);

            // Act
            var actual = await service.CleanMessageVoidBetIDAsync(message);

            // Assert
            Assert.Equal(expected, actual.Item1);
        }

        [Fact]
        public async Task ImportVoidBetIDAsync_WhenCalled_ReturnExpected()
        {
            // Arrange
            var fixture = new Fixture();
            var message = fixture.Create<IEnumerable<MessageInfoID>>();
            var commandResult = fixture.Create<CRM.Models.Common.CommandResult>();
            var command = fixture.Create<ImportVoidBetIDCommand>();
            var expected = new ValueTuple<CRM.Models.Common.CommandResult, ImportVoidBetIDCommand>(commandResult, command);
            var service = GetService();
            baseService.ExecuteNonQueryAsync(Arg.Any<ImportVoidBetIDCommand>()).Returns(commandResult);

            // Act
            var actual = await service.ImportVoidBetIDAsync(message, "UAT", 1);

            // Assert
            Assert.Equal(expected.Item1, actual.Item1);
        }

        #region Helpers

        private VoidingBetIDService GetService()
        {
            baseService = Substitute.For<IBaseService>();
            return new VoidingBetIDService(baseService);
        }

        #endregion Helpers
    }
}