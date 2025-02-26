namespace CRM.UnitTests.Criteria.VoidingSettledTickets
{
    using CRM.Criteria.VoidingSettledTickets;
    using Xunit;

    public class VoidingGetSettledTicketMessageCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new VoidingGetSettledTicketMessageCriteria();
            var expected = "Acc_CRM_VodingMessage_GetSettledTickets";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_ParamsAreValid_ReturnTrue()
        {
            // Arrange
            var criteria = new VoidingGetSettledTicketMessageCriteria
            {
                Message = "Message"
            };
            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}