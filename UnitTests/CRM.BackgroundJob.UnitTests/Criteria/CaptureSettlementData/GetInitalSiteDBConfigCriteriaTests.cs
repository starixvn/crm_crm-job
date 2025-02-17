namespace CRM.BackgroundJob.UnitTests.Criteria.CaptureSettlementData
{
    using CRM.BackgroundJob.Criteria.CaptureSettlementData;
    using Xunit;

    public class GetInitalSiteDBConfigCriteriaTests
    {
        [Fact]
        public void GetSettingKey_ReturnCorrectSettingKey()
        {
            // Arrange
            var criteria = new GetInitalSiteDBConfigCriteria();
            var expected = "MNC_SettlResultHist_Initial";

            // Action
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_ParamsAreValid_ReturnTrue()
        {
            // Arrange
            var criteria = new GetInitalSiteDBConfigCriteria();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
