namespace CRM.UnitTests.Criteria.VoidingBetID;

using CRM.Criteria.VoidingBetID;
using Xunit;

public class GetMessageVoidByBetIDCriteriaTests
{
    [Fact]
    public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
    {
        // Arrange
        var criteria = new GetMessageVoidByBetIDCriteria();
        var expected = "Acc_CRM_AutoFB_VoidByBetID_GetMessage";

        // Act
        var actual = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsValid_WhenCalled_ReturnTrue()
    {
        // Arrange
        var criteria = new GetMessageVoidByBetIDCriteria();

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.True(actual);
    }
}