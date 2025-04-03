namespace CRM.UnitTests.Criteria.VoidingSettledTickets;

using CRM.Criteria.VoidingSettledTickets;
using Xunit;

public class GetMessagesVoidingSettledCriteriaTests
{
    [Fact]
    public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
    {
        // Arrange
        var criteria = new GetMessagesVoidingSettledCriteria();
        var expected = "Acc_CRM_VoidingMsg_GetMessage";

        // Act
        var actual = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsValid_WhenCalled_ReturnTrue()
    {
        // Arrange
        var criteria = new GetMessagesVoidingSettledCriteria();

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.True(actual);
    }
}