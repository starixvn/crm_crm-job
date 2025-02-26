namespace CRM.UnitTests.Services.AutoFeedback
{
    using System.Threading.Tasks;
    using AutoFixture;
    using CRM.Services.AutoFeedback;
    using Fanex.Caching;
    using NSubstitute;
    using Xunit;

    public class JobRunningCheckerTests
    {
        private ICacheService cacheService;

        [Fact]
        public async Task IsRunning_WhenCalled_ReturnExpectedStatus()
        {
            // Arrange
            var fixture = new Fixture();
            var jobName = fixture.Create<string>();
            var job = GetJobRunningChecker();
            cacheService.GetAsync<bool>(Arg.Any<string>()).Returns(true);

            // Act
            var actual = await job.IsRunning(jobName);

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public async Task MarkRunning_WhenCalled_ExpectedCallSetAsync()
        {
            // Arrange
            var fixture = new Fixture();
            var jobName = fixture.Create<string>();
            var job = GetJobRunningChecker();

            // Act
            await job.MarkRunning(jobName);

            // Assert
            await cacheService.Received().SetAsync($"RunningJobs:{jobName}", true);
        }

        [Fact]
        public async Task ClearRunning_WhenCalled_ExpectedCallSetAsync()
        {
            // Arrange
            var fixture = new Fixture();
            var jobName = fixture.Create<string>();
            var job = GetJobRunningChecker();

            // Act
            await job.ClearRunning(jobName);

            // Assert
            await cacheService.Received().SetAsync($"RunningJobs:{jobName}", false);
        }

        #region Helpers

        private JobRunningChecker GetJobRunningChecker()
        {
            cacheService = Substitute.For<ICacheService>();
            return new JobRunningChecker(cacheService);
        }

        #endregion Helpers
    }
}