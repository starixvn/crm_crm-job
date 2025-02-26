namespace CRM.UnitTests.Services.NotificationCenter
{
    using AutoFixture;
    using CRM.Criteria.CaptureSettlementData;
    using CRM.Models.MessageNotificationCenter;
    using CRM.Services.Common;
    using CRM.Services.NotificationCenter;
    using NSubstitute;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;

    public class CaptureSettlementDataServiceTests
    {
        private IBaseService baseService;

        [Fact]
        public async Task GetInitialSiteDbConfigsAsync_WhenCalled_ReturnExpected()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture.Create<IEnumerable<CRM.Models.Common.SiteDbConfig>>();
            var service = GetService();
            baseService.GetDataAsync<CRM.Models.Common.SiteDbConfig>(Arg.Any<GetInitialSiteDBConfigCriteria>()).Returns(expected);

            // Act
            var actual = await service.GetInitialSiteDbConfigsAsync("dev");

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task GetSessionByDateAsync_WhenCalled_ReturnExpected()
        {
            // Arrange
            var fixture = new Fixture();
            var siteDbConfig = fixture.Create<CRM.Models.Common.SiteDbConfig>();
            var expected = fixture.Create<IEnumerable<SettleResultHistory>>();
            var service = GetService();
            baseService.GetDataByIpConfigAsync<SettleResultHistory>(siteDbConfig.AGIP, siteDbConfig.LinkedServerName, Arg.Any<GetSessionByDateCriteria>()).Returns(expected);

            // Act
            var actual = await service.GetSessionByDateAsync(siteDbConfig);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task InsertDataLoadAsync_WhenCalled_ReturnExpected()
        {
            // Arrange
            var service = GetService();

            // Act
            await service.InsertDataLoadAsync("dev", "jSRemotingRef");

            // Assert
            await baseService.Received(1).ExecuteNonQueryAsync(Arg.Any<InsertDataLoadCommand>());
        }

        #region Helpers

        private CaptureSettlementDataService GetService()
        {
            baseService = Substitute.For<IBaseService>();
            return new CaptureSettlementDataService(baseService);
        }

        #endregion Helpers
    }
}