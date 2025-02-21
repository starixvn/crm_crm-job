

namespace CRM.BackgroundJob.UnitTests.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using CRM.Models;
    using CRM.Services;
    using CRM.BackgroundJob.Criteria.SynchronizeLicensee;
    using CRM.BackgroundJob.Models.LicenseeInfo;
    using CRM.BackgroundJob.Services;
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
            var criteria = new GetLicenseeStatus();
            var service = GetService();

            baseService.GetDataAsync<LicenseeStatus>(Arg.Do<GetLicenseeStatus>(x => criteria = x)).Returns(expected);
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
                new LicenseeStatus
                {
                    LicenseeName = "1",
                    StatusId = "1"
                }
            };
            var criteria = new ImportLicenseeStatusCommand();
            var commandResult = fixture.Create<CommandResult>();
            var service = GetService();
            const string expected = "<ROOT><r Name='1' status='1'/></ROOT>";

            baseService.ExecuteNonQueryAsync(Arg.Do<ImportLicenseeStatusCommand>(x => criteria = x)).Returns(commandResult);
            // Act
            await service.ImportLicenseeStatus(licensees, "LIVE");

            // Assert
            Assert.Equal(expected.ToString(), criteria.Licensees.ToString());
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
