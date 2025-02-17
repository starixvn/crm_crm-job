namespace CRM.BackgroundJob.UnitTests.Criteria.ChatBot
{
    using Fanex.CRM.BackgroundJob.Criteria.ChatBot;
    using Xunit;

    public class EditChatBotVoidingMessageCommandTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new EditChatBotVoidingMessageCommand();
            var expected = "Acc_CRM_ChatBot_crmMessageImport_Upd";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void IsValid_ParamIsInvalid_ReturnFalse(string messages)
        {
            // Arrange
            var criteria = new EditChatBotVoidingMessageCommand
            {
                Messages = messages
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
            var criteria = new EditChatBotVoidingMessageCommand
            {
                Messages = "message"
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}