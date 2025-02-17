namespace CRM.BackgroundJob.UnitTests.Criteria.GenerateResultAmendment
{
    using CRM.BackgroundJob.Criteria.GenerateResultAmendment;
    using Xunit;

    public class GetPrepareGenNotificationCriteriaTests
    {
        [Fact]
        public void GetSettingKey_ReturnCorrectSettingKey()
        {
            // Arrange
            var criteria = new GetPrepareGenNotificationCriteria();
            var expected = "MNC_ResultAmendment_PrepareGen";

            // Action
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_ParamsAreValid_ReturnTrue()
        {
            // Arrange
            var criteria = new GetPrepareGenNotificationCriteria();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
