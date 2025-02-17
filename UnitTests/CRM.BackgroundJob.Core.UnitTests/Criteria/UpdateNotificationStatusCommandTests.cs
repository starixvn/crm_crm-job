namespace CRM.BackgroundJob.Core.UnitTests.Criteria
{
    using CRM.BackgroundJob.Core.Criteria;
    using Xunit;

    public class UpdateNotificationStatusCommandTests
    {
        [Fact]
        public void GetSettingKey_ReturnCorrectSettingKey()
        {
            // Arrange
            var criteria = new UpdateNotificationStatusCommand();
            var expected = "Acc_CRM_ChatBot_SendNotification_Status_Upd";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsInValid_WhenCalled_AlwaysReturnFalse()
        {
            // Arrange
            var criteria = new UpdateNotificationStatusCommand()
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
            var criteria = new UpdateNotificationStatusCommand()
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
