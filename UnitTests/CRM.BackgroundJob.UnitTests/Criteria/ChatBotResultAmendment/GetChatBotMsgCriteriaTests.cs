namespace CRM.BackgroundJob.UnitTests.Criteria.ChatBotResultAmendment
{
    using CRM.BackgroundJob.Criteria.ChatBot_ResultAmendment;
    using Xunit;

    public class GetChatBotMsgCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetChatBotMsgCriteria();
            var expected = "Acc_CRM_Chatbot_Msg_Sel";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_ParamIsValid_ReturnTrue()
        {
            // Arrange
            var criteria = new GetChatBotMsgCriteria
            {
               Messages = "Messages"
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
