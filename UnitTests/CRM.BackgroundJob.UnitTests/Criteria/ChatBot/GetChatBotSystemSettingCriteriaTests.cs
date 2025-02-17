namespace CRM.BackgroundJob.UnitTests.Criteria.ChatBot
{
    using Fanex.CRM.BackgroundJob.Criteria.ChatBot;
    using Xunit;

    public class GetChatBotSystemSettingCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetChatBotSystemSettingCriteria();
            var expected = "Acc_CRM_Chatbot_SystemSetting_Sel";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnTrue()
        {
            // Arrange
            var criteria = new GetChatBotSystemSettingCriteria
            {
                ServiceName = "chatbot",
                Environment = "Live"
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}