namespace CRM.UnitTests.Services.NotificationCenter
{
    using AutoFixture;
    using CRM.Criteria.GenerateLapNotification;
    using CRM.Models.MessageNotificationCenter;
    using CRM.Services.Common;
    using CRM.Services.NotificationCenter;
    using NSubstitute;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;

    public class GenerateLapNotificationServiceTests
    {
        private IBaseService baseService;

        [Fact]
        public async Task GetPrepareGenLapNotificationAsync_WhenCalled_ReturnExpected()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture.Create<IEnumerable<SettlementRemotingRefInfo>>();
            var service = GetService();
            baseService.GetDataAsync<SettlementRemotingRefInfo>(Arg.Any<GetPrepareGenLapNotificationCriteria>()).Returns(expected);

            // Act
            var actual = await service.GetPrepareGenLapNotificationAsync("dev");

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task InsertGenLapResultAmendmentAsync_WhenCalled_ReturnExpected()
        {
            // Arrange
            var fixture = new Fixture();
            var jSRemotingRef = fixture.Create<IEnumerable<SettlementRemotingRefInfo>>();
            var service = GetService();

            // Act
            await service.InsertGenLapResultAmendmentAsync(jSRemotingRef, "dev");

            // Assert
            await baseService.Received(1).ExecuteNonQueryAsync(Arg.Any<InsertGenLapResultAmendmentCommand>());
        }

        #region Helpers

        private GenerateLapNotificationService GetService()
        {
            baseService = Substitute.For<IBaseService>();
            return new GenerateLapNotificationService(baseService);
        }

        #endregion Helpers
    }
}