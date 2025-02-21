namespace CRM.BackgroundJob.UnitTests.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using CRM.BackgroundJob.Constants;
    using CRM.BackgroundJob.Criteria;
    using CRM.BackgroundJob.Criteria.AutoFeedback;
    using CRM.BackgroundJob.Models;
    using CRM.BackgroundJob.Services;
    using CRM.Services;
    using NSubstitute;
    using Xunit;

    public class SettingServiceTests
    {
        private IBaseService baseService;
        private readonly IFixture fixture = new Fixture();

        [Fact]
        public async Task GetAllSettingAsync_SettingIsNotExist_ReturnEmpty()
        {
            // Arrange
            var settings = new List<SettingItem>();
            var service = GetSettingService();
            baseService.GetDataAsync<SettingItem>(Arg.Any<GetAllSettingCriteria>()).Returns(settings);

            // Act
            var actual = await service.GetAllSettingAsync();

            // Assert
            Assert.Empty(actual);
        }

        [Fact]
        public async Task GetAllSettingAsync_CheckSettingRelation_ReturnExpected()
        {
            // Arrange
            var settings = new List<SettingItem>
            {
                new SettingItem { SettingId = 1, IsActive = true }
            };
            var relations = new List<SettingItem>
            {
                new SettingItem { SettingId = 1, ParentId = 2 }
            };
            var service = GetSettingService();
            baseService.GetDataAsync<SettingItem>(Arg.Any<GetAllSettingCriteria>()).Returns(settings);
            baseService.GetDataAsync<SettingItem>(Arg.Any<GetAllSettingRelationCriteria>()).Returns(relations);

            // Act
            var actual = await service.GetAllSettingAsync();

            // Assert
            Assert.Equal(settings, actual);
            Assert.Single(settings.First().ParentIds);
            Assert.Equal(2, settings.First().ParentIds.First());
        }

        [Fact]
        public async Task GetSystemSettingMappingAsync_WhenCalled_ReturnExpectedSetting()
        {
            // Arrange
            var settingNames = fixture.Create<string[]>();
            var settings = string.Join(",", settingNames);
            var expected = fixture.Create<IEnumerable<SystemSettingMapping>>();
            var criteria = new GetSystemSettingMappingCriteria();
            var service = GetSettingService();
            baseService.GetDataAsync<SystemSettingMapping>(Arg.Do<GetSystemSettingMappingCriteria>(x => criteria = x)).Returns(expected);

            // Act
            var actual = await service.GetSystemSettingMappingAsync(settingNames);

            // Assert
            Assert.Equal(expected, actual);
            Assert.Equal(settings, criteria.Settings);
        }

        [Fact]
        public async Task GetSiteSettingMappingAsync_WhenCalled_ReturnExpectedSetting()
        {
            // Arrange
            var expected = fixture.Create<IEnumerable<SiteSettingMapping>>();
            var criteria = new GetSystemSettingMappingCriteria();
            var service = GetSettingService();
            baseService.GetDataAsync<SiteSettingMapping>(Arg.Do<GetSystemSettingMappingCriteria>(x => criteria = x)).Returns(expected);

            // Act
            var actual = await service.GetSiteSettingMappingAsync();

            // Assert
            Assert.Equal(expected, actual);
            Assert.Equal(SettingName.Site, criteria.Settings);
        }

        #region Helpers

        private SettingService GetSettingService()
        {
            baseService = Substitute.For<IBaseService>();
            return new SettingService(baseService);
        }

        #endregion Helpers
    }
}