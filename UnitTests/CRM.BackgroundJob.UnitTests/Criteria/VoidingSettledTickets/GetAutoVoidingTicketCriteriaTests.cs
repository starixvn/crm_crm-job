namespace CRM.BackgroundJob.UnitTests.Criteria.VoidingSettledTickets
{
    using CRM.BackgroundJob.Criteria.VoidingSettledTickets;
    using Xunit;

    public class GetAutoVoidingTicketCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetAutoVoidingTicketCriteria
            {
                MessageIDs = "MessageIDs",
                ConfigUAT = true,
                ConfigPRO = true,
                Environment = "LIVE"
            };
            var expected = "Acc_CRM_AutoVoidingTicketSel";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnTrue()
        {
            // Arrange
            var criteria = new GetAutoVoidingTicketCriteria { 
                MessageIDs = "MessageIDs"
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void IsValid_ParamIsInvalid_ReturnFalse(string messageIDs)
        {
            // Arrange
            var criteria = new GetAutoVoidingTicketCriteria
            {
                MessageIDs = messageIDs
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.False(actual);
        }
    }
}
