namespace CRM.UnitTests.Criteria.ChatBot
{
    using CRM.Criteria.ChatBot;
    using Xunit;

    public class GetChatBotVoidingMessageCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetChatBotVoidingMessageCriteria();
            var expected = "Acc_CRM_Chatbot_AbnormalBets_GetMessage";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnTrue()
        {
            // Arrange
            var criteria = new GetChatBotVoidingMessageCriteria { BatchSize = 1, LastVoidingHistoryCollectionId = 1};

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}