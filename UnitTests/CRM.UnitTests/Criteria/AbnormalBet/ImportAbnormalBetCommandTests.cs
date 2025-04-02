namespace CRM.UnitTests.Criteria.AbnormalBet;

using CRM.Criteria.AbnormalBet;
using Xunit;

public class ImportAbnormalBetCommandTests
{
    [Fact]
    public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
    {
        // Arrange
        var criteria = new ImportAbnormalBetCommand();
        var expected = "Acc_CRM_AutoFB_AbnormalBets_Import";

        // Act
        var actual = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsValid_WhenCalled_ReturnTrue()
    {
        // Arrange
        var criteria = new ImportAbnormalBetCommand();

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.True(actual);
    }
}