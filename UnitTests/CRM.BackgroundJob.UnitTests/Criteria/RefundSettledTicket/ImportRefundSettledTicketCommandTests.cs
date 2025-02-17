namespace CRM.BackgroundJob.UnitTests.Criteria.RefundSettledTicket
{
    using CRM.BackgroundJob.Criteria.RefundSettledTicket;
    using Xunit;

    public class ImportRefundSettledTicketCommandTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new ImportRefundSettledTicketCommand();
            var expected = "Acc_CRM_AutoFB_RefundSettledTickets_Import";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnTrue()
        {
            // Arrange
            var criteria = new ImportRefundSettledTicketCommand();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
