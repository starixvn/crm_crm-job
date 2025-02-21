namespace CRM.BackgroundJob.Core.UnitTests.Criteria.Announcement
{
    using CRM.Criteria.Announcement;
    using Xunit;

    public class GetAnnouncementNotificationToSendCriteriaTests
    {
        [Fact]
        public void GetSettingKey_ReturnCorrectSettingKey()
        {
            // Arrange
            var criteria = new GetAnnouncementNotificationToSendCriteria();
            var expected = "Acc_CRM_Chatbot_Announcement_Notification_Sel";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_AlwaysReturnTrue()
        {
            // Arrange
            var criteria = new GetAnnouncementNotificationToSendCriteria();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
