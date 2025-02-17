namespace CRM.BackgroundJob.UnitTests.Criteria
{
    using CRM.BackgroundJob.Criteria;
    using Xunit;

    public class GetSystemSettingCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetSystemSettingCriteria();
            var expected = "Acc_CRM_AutoFB_SystemSetting_Sel";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnTrue()
        {
            // Arrange
            var criteria = new GetSystemSettingCriteria
            {
                ServiceName = "ServiceName",
                Environment = "Dev"
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
