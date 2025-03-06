namespace CRM.UnitTests.Services.AutoFeedback
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using CRM.Criteria.SynchronizeLicensee;
    using CRM.Models.LicenseeInfo;
    using CRM.Services.AutoFeedback;
    using CRM.Services.Common;
    using NSubstitute;
    using Xunit;

    public class SynchronizeLicenseeServiceTests
    {
        private IBaseService baseService;

        private readonly IFixture fixture = new Fixture();

        [Fact]
        public async Task GetOICLicenseeInfo_WhenCalled_ReturnExpectedLicensee()
        {
            // Arrange
            var expected = fixture.Create<IEnumerable<LicenseeStatus>>();
            var service = GetService();

            baseService.GetDataAsync<LicenseeStatus>(Arg.Do<GetLicenseeStatus>(x => _ = x)).Returns(expected);
            // Act
            var actual = await service.GetOICLicenseeInfo();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ImportLicenseeStatus_WhenCalled_ReturnExpectedLicensee()
        {
            // Arrange
            var licensees = new List<LicenseeStatus>
            {
                new()
                {
                    LicenseeName = "1",
                    StatusId = "1"
                }
            };
            var criteria = new ImportLicenseeStatusCommand();
            var commandResult = fixture.Create<CRM.Models.Common.CommandResult>();
            var service = GetService();
            const string expected = "<ROOT><r Name='1' status='1'/></ROOT>";

            baseService.ExecuteNonQueryAsync(Arg.Do<ImportLicenseeStatusCommand>(x => criteria = x)).Returns(commandResult);
            // Act
            await service.ImportLicenseeStatus(licensees, "LIVE");

            // Assert
            Assert.Equal(expected, criteria.Licensees);
        }

        #region Helpers

        private SynchronizeLicenseeService GetService()
        {
            baseService = Substitute.For<IBaseService>();

            return new SynchronizeLicenseeService(baseService);
        }

        #endregion Helpers
    }
}