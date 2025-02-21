namespace CRM.BackgroundJob.UnitTests.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using CRM.BackgroundJob.Criteria.CaptureSettlementData;
    using CRM.BackgroundJob.Models.MessageNotificationCenter;
    using CRM.BackgroundJob.Services;
    using CRM.Models;
    using CRM.Services;
    using NSubstitute;
    using Xunit;

    public class CaptureSettlementDataServiceTests
    {
        private IBaseService baseService;

        [Fact]
        public async Task GetInitialSiteDbConfigsAsync_WhenCalled_ReturnExpected()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture.Create<IEnumerable<SiteDbConfig>>();
            var service = GetService();
            baseService.GetDataAsync<SiteDbConfig>(Arg.Any<GetInitalSiteDBConfigCriteria>()).Returns(expected);

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
            var siteDbConfig = fixture.Create<SiteDbConfig>();
            var expected = fixture.Create<IEnumerable<SettlResultHistory>>();
            var service = GetService();
            baseService.GetDataByIpConfigAsync<SettlResultHistory>(siteDbConfig.AGIP, siteDbConfig.LinkedServerName, Arg.Any<GetSessionByDateCriteria>()).Returns(expected);

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
