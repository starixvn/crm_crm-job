namespace CRM.BackgroundJob.UnitTests.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using CRM.BackgroundJob.Core.Services;
    using CRM.BackgroundJob.Criteria.GenerateResultAmendment;
    using CRM.BackgroundJob.Models.MessageNotificationCenter;
    using CRM.BackgroundJob.Services;
    using NSubstitute;
    using Xunit;

    public class GenerateResultAmendmentServiceTests
    {
        private IBaseService baseService;

        [Fact]
        public async Task GetPrepareGenNotificationAsync_WhenCalled_ReturnExpected()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture.Create<IEnumerable<SettlementRemotingRefInfo>>();
            var service = GetService();
            baseService.GetDataAsync<SettlementRemotingRefInfo>(Arg.Any<GetPrepareGenNotificationCriteria>()).Returns(expected);

            // Act
            var actual = await service.GetPrepareGenNotificationAsync("dev");

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task InsertGenResultAmendmentAsync_WhenCalled_ReturnExpected()
        {
            // Arrange
            var fixture = new Fixture();
            var jSRemotingRef = fixture.Create<IEnumerable<SettlementRemotingRefInfo>>();
            var service = GetService();

            // Act
            await service.InsertGenResultAmendmentAsync("dev", jSRemotingRef);

            // Assert
            await baseService.Received(1).ExecuteNonQueryAsync(Arg.Any<InsertGenResultAmendmentCommand>());
        }

        #region Helpers
        private GenerateResultAmendmentService GetService()
        {
            baseService = Substitute.For<IBaseService>();
            return new GenerateResultAmendmentService(baseService);
        }
        #endregion Helpers
    }
}
