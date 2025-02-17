namespace CRM.BackgroundJob.UnitTests.Criteria
{
    using CRM.BackgroundJob.Criteria;
    using Xunit;

    public class GetSystemSettingNotificationCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetSystemSettingNotificationCriteria();
            var expected = "MNC_Notification_SystemSettingSel";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnTrue()
        {
            // Arrange
            var criteria = new GetSystemSettingNotificationCriteria
            {
                Environment = "Dev"
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
