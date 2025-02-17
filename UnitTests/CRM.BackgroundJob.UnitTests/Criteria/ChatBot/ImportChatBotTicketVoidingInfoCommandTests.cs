namespace CRM.BackgroundJob.UnitTests.Criteria.ChatBot
{
    using Fanex.CRM.BackgroundJob.Criteria.ChatBot;
    using Xunit;

    public class ImportChatBotTicketVoidingInfoCommandTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new ImportChatBotTicketVoidingInfoCommand();
            var expected = "Acc_CRM_Chatbot_AbnormalBets_Import";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData(" ", " ")]
        public void IsValid_ParamIsInvalid_ReturnFalse(string messages, string XmlData)
        {
            // Arrange
            var criteria = new ImportChatBotTicketVoidingInfoCommand
            {
                XmlData = XmlData
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
            var criteria = new ImportChatBotTicketVoidingInfoCommand
            {
                XmlData = "XmlData",
                Environment = "Environment"
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}