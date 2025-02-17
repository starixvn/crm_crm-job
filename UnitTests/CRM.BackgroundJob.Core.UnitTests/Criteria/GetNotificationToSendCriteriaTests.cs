namespace CRM.BackgroundJob.Core.UnitTests.Criteria
{
    using CRM.BackgroundJob.Core.Criteria;
    using Xunit;

    public class GetNotificationToSendCriteriaTests
    {
        [Fact]
        public void GetSettingKey_ReturnCorrectSettingKey()
        {
            // Arrange
            var criteria = new GetNotificationToSendCriteria();
            var expected = "Acc_CRM_Chatbot_SendNotification";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_AlwaysReturnTrue()
        {
            // Arrange
            var criteria = new GetNotificationToSendCriteria();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
