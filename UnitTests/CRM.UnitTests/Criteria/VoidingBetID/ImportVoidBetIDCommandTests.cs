namespace CRM.UnitTests.Criteria.VoidingBetID;

using CRM.Criteria.VoidingBetID;
using Xunit;

public class ImportVoidBetIDCommandTests
{
    [Fact]
    public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
    {
        // Arrange
        var criteria = new ImportVoidBetIDCommand();
        var expected = "Acc_CRM_AutoFB_VoidByBetID_Import";

        // Act
        var actual = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsValid_WhenCalled_ReturnTrue()
    {
        // Arrange
        var criteria = new ImportVoidBetIDCommand();

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.True(actual);
    }
}