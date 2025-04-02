namespace CRM.UnitTests.Criteria.AutoFbRebateRetention;

using CRM.Criteria.AutoFbRebateRetention;
using Xunit;

public class ImportAutoFbRebateRetentionCommandTests
{
    [Fact]
    public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
    {
        // Arrange
        var criteria = new ImportAutoFbRebateRetentionCommand();
        var expected = "Acc_CRM_AutoFB_RebateRetention_Import";

        // Act
        var actual = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsValid_WhenCalled_ReturnTrue()
    {
        // Arrange
        var criteria = new ImportAutoFbRebateRetentionCommand
        {
            XmlData = "XmlData",
            NewLogSID = "1",
            Environment = "dev"
        };

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.True(actual);
    }
}