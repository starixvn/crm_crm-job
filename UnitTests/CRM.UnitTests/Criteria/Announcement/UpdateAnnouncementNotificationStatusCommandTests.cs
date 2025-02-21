namespace CRM.UnitTests.Criteria.Announcement
{
    using CRM.Criteria.Announcement;
    using Xunit;

    public class UpdateAnnouncementNotificationStatusCommandTests
    {
        [Fact]
        public void GetSettingKey_ReturnCorrectSettingKey()
        {
            // Arrange
            var criteria = new UpdateAnnouncementNotificationStatusCommand();
            var expected = "Acc_CRM_ChatBot_Announcement_Notification_UpdStatus";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsInValid_WhenCalled_AlwaysReturnFalse()
        {
            // Arrange
            var criteria = new UpdateAnnouncementNotificationStatusCommand()
            {
                XmlData = null
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void IsValid_WhenCalled_AlwaysReturnTrue()
        {
            // Arrange
            var criteria = new UpdateAnnouncementNotificationStatusCommand()
            {
                XmlData = "test"
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
