namespace CRM.UnitTests.Criteria.Common;

using CRM.Constants;
using CRM.Criteria.Common;
using Xunit;

public class GetAllSettingCriteriaTests
{
    [Fact]
    public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
    {
        // Arrange
        var criteria = new GetAllSettingCriteria();
        var expected = "Acc_CRM_SettingSel";

        // Act
        var actual = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expected, actual);
        Assert.Equal(BackgroundJobConstants.UserRole, criteria.UserRole);
    }

    [Fact]
    public void IsValid_WhenCalled_ReturnTrue()
    {
        // Arrange
        var criteria = new GetAllSettingCriteria();

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.True(actual);
    }
}