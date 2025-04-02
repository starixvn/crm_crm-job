namespace CRM.UnitTests.Criteria.TurnoverControl;

using CRM.Criteria.TurnoverControl;
using Xunit;

public class GetTOControlSystemSettingCriteriaTest
{
    [Fact]
    public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
    {
        // Arrange
        var criteria = new GetTOControlSystemSettingCriteria();
        var expected = "Acc_CRM_TOControl_SystemSettingSel";

        // Act
        var actual = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsValid_WhenCalled_ReturnTrue()
    {
        // Arrange
        var criteria = new GetTOControlSystemSettingCriteria();

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.True(actual);
    }
}