namespace CRM.UnitTests.Services
{
    using AutoFixture;
    using CRM.Criteria;
    using CRM.Models;
    using CRM.Repositories;
    using CRM.Services;
    using Fanex.Caching;
    using Fanex.Data.Repository;
    using NSubstitute;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class BaseServiceTests
    {
        private IDynamicRepository dynamicRepository;
        private IBaseRepository baseRepository;
        private ICacheService cacheService;
        private ICrmLogService logService;
        private readonly IFixture fixture = new Fixture();

        #region GetDataAsync

        [Fact]
        public async Task GetDataAsync_CriteriaIsNull_ReturnEmptyList()
        {
            // Arrange
            var baseService = GetBaseService();

            // Act
            var actual = await baseService.GetDataAsync<int>(null);

            // Assert
            Assert.Empty(actual);
        }

        [Fact]
        public async Task GetDataAsync_CriteriaIsInvalid_ReturnEmptyList()
        {
            // Arrange
            var criteria = Substitute.For<ICriteria>();
            criteria.IsValid().Returns(false);
            var baseService = GetBaseService();

            // Act
            var actual = await baseService.GetDataAsync<int>(criteria);

            // Assert
            Assert.Empty(actual);
        }

        [Fact]
        public async Task GetDataAsync_CriteriaIsValid_ReturnExpectedData()
        {
            // Arrange
            var expected = new List<int> { 1, 2 };
            var criteria = Substitute.For<ICriteria>();
            criteria.IsValid().Returns(true);
            var baseService = GetBaseService();
            dynamicRepository.FetchAsync<int>(criteria).Returns(expected);

            // Act
            var actual = await baseService.GetDataAsync<int>(criteria);

            // Assert
            Assert.NotEmpty(actual);
            Assert.Equal(expected, actual);
        }

        #endregion GetDataAsync

        #region GetDataEntityAsync

        [Fact]
        public async Task GetDataEntityAsync_CriteriaIsNull_ReturnDefaultValue()
        {
            // Arrange
            var baseService = GetBaseService();

            // Act
            var actual = await baseService.GetDataEntityAsync<int>(null);

            // Assert
            Assert.Equal(0, actual);
        }

        [Fact]
        public async Task GetDataEntityAsync_CriteriaIsInvalid_ReturnDefaultValue()
        {
            // Arrange
            var criteria = Substitute.For<ICriteria>();
            criteria.IsValid().Returns(false);
            var baseService = GetBaseService();

            // Act
            var actual = await baseService.GetDataEntityAsync<int>(criteria);

            // Assert
            Assert.Equal(0, actual);
        }

        [Fact]
        public async Task GetDataEntityAsync_CriteriaIsValid_ReturnExpectedData()
        {
            // Arrange
            var expected = 1;
            var criteria = Substitute.For<ICriteria>();
            criteria.IsValid().Returns(true);
            var baseService = GetBaseService();
            dynamicRepository.GetAsync<int>(criteria).Returns(expected);

            // Act
            var actual = await baseService.GetDataEntityAsync<int>(criteria);

            // Assert
            Assert.Equal(expected, actual);
        }

        #endregion GetDataEntityAsync

        #region GetSiteDbConfigAsync

        [Fact]
        public async Task GetSiteDbConfigAsync_CacheIsNull_ReturnExactValue()
        {
            // Arrange
            var siteConfigs = fixture.Create<IEnumerable<SiteDbConfig>>();
            var cache = Enumerable.Empty<SiteDbConfig>();
            var baseService = GetBaseService();
            cacheService.Get<IEnumerable<SiteDbConfig>>(Arg.Any<string>()).Returns(cache);
            dynamicRepository.FetchAsync<SiteDbConfig>(Arg.Any<GetAllSiteDbConfigCriteria>()).Returns(siteConfigs);

            // Act
            var actual = await baseService.GetSiteDbConfigAsync();

            // Assert
            Assert.Equal(siteConfigs, actual);
        }

        [Fact]
        public async Task GetSiteDbConfigAsync_CacheIsNotNull_ReturnExactValue()
        {
            // Arrange
            var siteConfigs = fixture.Create<IEnumerable<SiteDbConfig>>();
            var criteria = Substitute.For<ICriteria>();
            criteria.IsValid().Returns(true);
            var baseService = GetBaseService();
            cacheService.Get<IEnumerable<SiteDbConfig>>(Arg.Any<string>()).Returns(siteConfigs);
            // Act
            var actual = await baseService.GetSiteDbConfigAsync();

            // Assert
            Assert.Equal(siteConfigs, actual);
        }

        #endregion GetSiteDbConfigAsync

        #region GetDataFromSiteDbAsync

        [Fact]
        public async Task GetDataFromSiteDbAsync_ReturnExactValue()
        {
            // Arrange
            var siteConfigs = fixture.Create<IEnumerable<SiteDbConfig>>();
            var result = fixture.Create<IEnumerable<int>>();
            var expected = new List<int>();
            foreach (var _ in siteConfigs)
            {
                expected.AddRange(result);
            }
            var criteria = Substitute.For<IBaseCriteriaExtend>();
            criteria.IsValid().Returns(true);
            var baseService = GetBaseService();
            baseService.WhenForAnyArgs(x => x.GetSiteDbConfigAsync()).DoNotCallBase();
            baseService.GetSiteDbConfigAsync().Returns(siteConfigs);
            baseService.WhenForAnyArgs(x => x.GetDataByIpConfigAsync<int>(Arg.Any<string>(), Arg.Any<string>(), criteria)).DoNotCallBase();
            baseService.GetDataByIpConfigAsync<int>(Arg.Any<string>(), Arg.Any<string>(), criteria).Returns(result);
            // Act
            var actual = await baseService.GetDataFromSiteDbAsync<int>(criteria);

            // Assert
            Assert.Equal(expected, actual);
        }

        #endregion GetDataFromSiteDbAsync

        #region FetchMultipleAsync

        [Fact]
        public async Task FetchMultipleAsync_CriteriaIsNull_ReturnTupleWithEmptyList()
        {
            // Arrange
            var baseService = GetBaseService();

            // Act
            var actual = await baseService.FetchMultipleAsync<int, int>(null);

            // Assert
            Assert.Empty(actual.Item1);
            Assert.Empty(actual.Item2);
        }

        [Fact]
        public async Task FetchMultipleAsync_CriteriaIsInvalid_ReturnTupleWithEmptyList()
        {
            // Arrange
            var criteria = Substitute.For<ICriteria>();
            criteria.IsValid().Returns(false);
            var baseService = GetBaseService();

            // Act
            var actual = await baseService.FetchMultipleAsync<int, int>(criteria);

            // Assert
            Assert.Empty(actual.Item1);
            Assert.Empty(actual.Item2);
        }

        [Fact]
        public async Task FetchMultipleAsync_CriteriaIsValid_ReturnExpectedData()
        {
            // Arrange
            var expected = new ValueTuple<IEnumerable<int>, IEnumerable<int>>(new List<int> { 1, 2 }, new List<int> { 1, 2 });
            var criteria = Substitute.For<ICriteria>();
            criteria.IsValid().Returns(true);
            var baseService = GetBaseService();
            dynamicRepository.FetchMultipleAsync<int, int>(criteria).Returns(expected);

            // Act
            var actual = await baseService.FetchMultipleAsync<int, int>(criteria);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task FetchMultipleAsync_WithThreeDataSetCriteriaIsNull_ReturnTupleWithEmptyList()
        {
            // Arrange
            var baseService = GetBaseService();

            // Act
            var actual = await baseService.FetchMultipleAsync<int, int, int>(null);

            // Assert
            Assert.Empty(actual.Item1);
            Assert.Empty(actual.Item2);
            Assert.Empty(actual.Item3);
        }

        [Fact]
        public async Task FetchMultipleAsync_WithThreeDataCriteriaIsInvalid_ReturnTupleWithEmptyList()
        {
            // Arrange
            var criteria = Substitute.For<ICriteria>();
            criteria.IsValid().Returns(false);
            var baseService = GetBaseService();

            // Act
            var actual = await baseService.FetchMultipleAsync<int, int, int>(criteria);

            // Assert
            Assert.Empty(actual.Item1);
            Assert.Empty(actual.Item2);
            Assert.Empty(actual.Item3);
        }

        [Fact]
        public async Task FetchMultipleAsync_WithThreeDataCriteriaIsValid_ReturnExpectedData()
        {
            // Arrange
            var expected = new ValueTuple<IEnumerable<int>, IEnumerable<int>, IEnumerable<int>>(new List<int> { 1, 2 }, new List<int> { 1, 2 }, new List<int> { 1, 2 });
            var criteria = Substitute.For<ICriteria>();
            criteria.IsValid().Returns(true);
            var baseService = GetBaseService();
            dynamicRepository.FetchMultipleAsync<int, int, int>(criteria).Returns(expected);

            // Act
            var actual = await baseService.FetchMultipleAsync<int, int, int>(criteria);

            // Assert
            Assert.Equal(expected, actual);
        }

        #endregion FetchMultipleAsync

        #region ExecuteNonQueryAsync

        [Fact]
        public async Task ExecuteNonQueryAsync_CriteriaIsNull_ReturnDefaultCommandResult()
        {
            // Arrange
            var expected = new CommandResult();
            var baseService = GetBaseService();

            // Act
            var actual = await baseService.ExecuteNonQueryAsync(null);

            // Assert
            Assert.Equal(expected.Code, actual.Code);
            Assert.Equal(expected.IsSuccessful, actual.IsSuccessful);
            Assert.Equal(expected.Message, actual.Message);
        }

        [Fact]
        public async Task ExecuteNonQueryAsync_CriteriaIsInvalid_ReturnDefaultCommandResult()
        {
            // Arrange
            var expected = new CommandResult();
            var criteria = Substitute.For<INonQueryCommand>();
            criteria.IsValid().Returns(false);
            var baseService = GetBaseService();

            // Act
            var actual = await baseService.ExecuteNonQueryAsync(criteria);

            // Assert
            Assert.Equal(expected.Code, actual.Code);
            Assert.Equal(expected.IsSuccessful, actual.IsSuccessful);
            Assert.Equal(expected.Message, actual.Message);
        }

        [Fact]
        public async Task ExecuteNonQueryAsync_CriteriaIsValid_ExpectedExecuteQuery()
        {
            // Arrange
            var criteria = Substitute.For<INonQueryCommand>();
            criteria.IsValid().Returns(true);
            var baseService = GetBaseService();

            // Act
            await baseService.ExecuteNonQueryAsync(criteria);

            // Assert
            await baseRepository.Received().ExecuteNonQueryAsync(Arg.Is<INonQueryCommand>(x => x.IsValid()));
        }

        #endregion ExecuteNonQueryAsync

        #region Helpers

        private BaseService GetBaseService()
        {
            dynamicRepository = Substitute.For<IDynamicRepository>();
            baseRepository = Substitute.For<IBaseRepository>();
            cacheService = Substitute.For<ICacheService>();
            logService = Substitute.For<ICrmLogService>();
            var service = Substitute.ForPartsOf<BaseService>(dynamicRepository, baseRepository, cacheService, logService);

            return service;
        }

        #endregion Helpers
    }
}