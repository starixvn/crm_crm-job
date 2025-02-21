namespace CRM.UnitTests.Repositories
{
    using System.Collections;
    using System.Threading.Tasks;
    using CRM.Models;
    using CRM.Repositories;
    using Fanex.Data;
    using Fanex.Data.Repository;
    using NSubstitute;
    using Xunit;

    public class BaseRepositoryTests
    {
        private readonly IObjectDb objectDb;

        public BaseRepositoryTests()
        {
            DbSettingProviderManager
            .StartTestingSession()
            .Run();

            objectDb = Substitute.For<IObjectDb>();
            ObjectDbFactory.Mock((key, time, ip) => objectDb);
        }

        #region ExecuteNonQueryAsync

        [Fact]
        public async Task ExecuteNonQueryAsync_CriteriaNotNull_CallExecuteNonQueryAsync()
        {
            // Arrange
            var criteria = FakeCriteria();
            var repository = new BaseRepository();

            // Action
            await repository.ExecuteNonQueryAsync(criteria);

            // Assert
            await objectDb.Received().ExecuteNonQueryAsync(criteria);
        }

        [Fact]
        public async Task ExecuteNonQueryAsync_OutputParams_ReturnAsResult()
        {
            // Arrange
            var repository = new BaseRepository();
            var criteria = FakeCriteria();
            var outputParams = FakeOutputParamsSuccess();
            var expected = GetSuccessResult();
            objectDb.GetOutputParams().Returns(outputParams);

            // Action
            var result = await repository.ExecuteNonQueryAsync(criteria);

            // Assert
            Assert.Equal(expected.Code, result.Code);
            Assert.Equal(expected.Message, result.Message);
        }

        #endregion ExecuteNonQueryAsync

        #region Helpers

        private static INonQueryCommand FakeCriteria()
        {
            var criteria = Substitute.For<INonQueryCommand>();

            criteria.GetSettingKey().Returns("key");
            criteria.IsValid().Returns(true);

            return criteria;
        }

        private static Hashtable FakeOutputParamsSuccess()
        {
            return new Hashtable
            {
                { "errcode", 0 },
                { "errmsg", string.Empty },
            };
        }

        private CommandResult GetSuccessResult()
        {
            return new CommandResult
            {
                Code = 0,
                Message = string.Empty
            };
        }

        #endregion Helpers
    }
}
