namespace CRM.BackgroundJob.Core.UnitTests.Criteria
{
    using CRM.Criteria;
    using Xunit;

    public class GetAllSiteDbConfigCriteriaTests
    {
        [Fact]
        public void GetSettingKey_ReturnCorrectSettingKey()
        {
            // Arrange
            var criteria = new GetAllSiteDbConfigCriteria();
            var expected = "Acc_SiteDBConfigSel";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_AlwaysReturnTrue()
        {
            // Arrange
            var criteria = new GetAllSiteDbConfigCriteria();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
