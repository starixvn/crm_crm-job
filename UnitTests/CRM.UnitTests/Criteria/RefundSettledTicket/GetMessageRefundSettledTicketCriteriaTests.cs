namespace CRM.UnitTests.Criteria.RefundSettledTicket;

using CRM.Criteria.RefundSettledTicket;
using Xunit;

public class GetMessageRefundSettledTicketCriteriaTests
{
    [Fact]
    public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
    {
        // Arrange
        var criteria = new GetMessageRefundSettledTicketCriteria();
        var expected = "Acc_CRM_AutoFB_RefundSettledTickets_GetMessage";

        // Act
        var actual = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsValid_WhenCalled_ReturnTrue()
    {
        // Arrange
        var criteria = new GetMessageRefundSettledTicketCriteria();

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.True(actual);
    }
}