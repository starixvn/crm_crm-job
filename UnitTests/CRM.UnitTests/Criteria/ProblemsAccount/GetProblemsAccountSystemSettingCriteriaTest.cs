

namespace CRM.UnitTests.Criteria.ProblemsAccount;

using CRM.Criteria.ProblemsAccount;
using Xunit;

public class GetProblemsAccountSystemSettingCriteriaTest
{
    [Fact]
    public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
    {
        // Arrange
        var criteria = new GetProblemsAccountSystemSettingCriteria();
        var expected = "Acc_CRM_ProblemAccount_SystemSettingSel";

        // Act
        var actual = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsValid_ParamIsValid_ReturnTrue()
    {
        // Arrange
        var criteria = new GetProblemsAccountSystemSettingCriteria();

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.True(actual);
    }
}