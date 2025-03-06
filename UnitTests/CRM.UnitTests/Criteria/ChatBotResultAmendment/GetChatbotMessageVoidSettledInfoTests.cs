namespace CRM.UnitTests.Criteria.ChatBotResultAmendment
{
    using CRM.Criteria.ChatBot_ResultAmendment;
    using Xunit;

    public class GetChatbotMessageVoidSettledInfoTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetChatbotMessageVoidSettledInfo();
            var expected = "Acc_CRM_Chatbot_VodingMessage_Sel";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_ParamIsValid_ReturnTrue()
        {
            // Arrange
            var criteria = new GetChatbotMessageVoidSettledInfo
            {
                Message = "<root></root>"
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
