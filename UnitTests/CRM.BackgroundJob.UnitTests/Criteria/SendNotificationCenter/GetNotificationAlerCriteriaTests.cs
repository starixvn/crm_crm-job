namespace CRM.BackgroundJob.UnitTests.Criteria.SendNotificationCenter
{
    using CRM.BackgroundJob.Criteria.SendNotificationCenter;
    using Xunit;

    public class GetNotificationAlerCriteriaTests
    {
        [Fact]
        public void GetSettingKey_ReturnCorrectSettingKey()
        {
            // Arrange
            var criteria = new GetNotificationAlertCriteria();
            var expected = "MNC_Notification_AlertSel";

            // Action
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_ParamsAreValid_ReturnTrue()
        {
            // Arrange
            var criteria = new GetNotificationAlertCriteria();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
