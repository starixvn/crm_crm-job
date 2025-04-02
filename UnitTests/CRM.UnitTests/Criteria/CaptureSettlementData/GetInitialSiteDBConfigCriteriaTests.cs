namespace CRM.UnitTests.Criteria.CaptureSettlementData;

using CRM.Criteria.CaptureSettlementData;
using Xunit;

public class GetInitialSiteDBConfigCriteriaTests
{
    [Fact]
    public void GetSettingKey_ReturnCorrectSettingKey()
    {
        // Arrange
        var criteria = new GetInitialSiteDBConfigCriteria();
        var expected = "MNC_SettlResultHist_Initial";

        // Action
        var actual = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsValid_ParamsAreValid_ReturnTrue()
    {
        // Arrange
        var criteria = new GetInitialSiteDBConfigCriteria();

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.True(actual);
    }
}