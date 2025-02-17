namespace CRM.BackgroundJob.UnitTests.Criteria.ChatBotResultAmendment
{
    using CRM.BackgroundJob.Criteria.ChatBot_ResultAmendment;
    using Xunit;

    public class VoidingMessageGetSettledTicketTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new VoidingMessageGetSettledTicket();
            var expected = "Acc_CRM_ChatBot_VodingMessage_GetSettledTickets";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_ParamIsValid_ReturnTrue()
        {
            // Arrange
            var criteria = new VoidingMessageGetSettledTicket
            {
                Message = "message"
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]

        public void IsValid_ParamIsInvalid_ReturnFalse(string message)
        {
            // Arrange
            var criteria = new VoidingMessageGetSettledTicket
            {
                Message = message
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.False(actual);
        }
    }
}

