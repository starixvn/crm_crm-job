namespace CRM.UnitTests.Criteria.ChatBot
{
    using CRM.Criteria.ChatBot;
    using Xunit;

    public class GetChatBotVoidingBetIdMessageCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetChatBotVoidingBetIdMessageCriteria();
            var expected = "Acc_CRM_Chatbot_AbnormalBets_VoidByBetID_GetMessage";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_ParamIsInvalid_ReturnFalse()
        {
            // Arrange
            var criteria = new GetChatBotVoidingBetIdMessageCriteria
            {
                BatchSize = -1
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void IsValid_ParamIsValid_ReturnTrue()
        {
            // Arrange
            var criteria = new GetChatBotVoidingBetIdMessageCriteria
            {
                BatchSize = 1
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
