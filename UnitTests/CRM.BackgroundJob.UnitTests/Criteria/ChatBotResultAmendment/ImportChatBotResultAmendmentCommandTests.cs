namespace CRM.BackgroundJob.UnitTests.Criteria.ChatBotResultAmendment
{
    using CRM.BackgroundJob.Criteria.ChatBot_ResultAmendment;
    using Xunit;

    public class ImportChatBotResultAmendmentCommandTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new ImportChatBotResultAmendmentCommand();
            var expected = "Acc_CRM_Chatbot_ResultAmendment_Import";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnTrue()
        {
            // Arrange
            var criteria = new ImportChatBotResultAmendmentCommand
            {
                EventData = "EventData",
                TicketData = "TicketData",
                MessageData = "TicketData",
                TargetMessage = "TargetMessage",
                InfoMessage = "InfoMessage",
                Environment = "LIVE",
                ServerName = "ServerName",
                ErrCode = 0,
                ErrMsg = ""
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}