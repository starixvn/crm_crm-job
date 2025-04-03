namespace CRM.UnitTests.Criteria.ProblemsAccount;

using CRM.Criteria.ProblemsAccount;
using Xunit;

public class GetProblemsAccountCustCategoryCriteriaTest
{
    [Fact]
    public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
    {
        // Arrange
        var criteria = new GetProblemsAccountCustCategoryCriteria();
        var expected = "Acc_CRM_ProblemAccount_CustCategorySel";

        // Act
        var actual = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsValid_ParamIsValid_ReturnTrue()
    {
        // Arrange
        var criteria = new GetProblemsAccountCustCategoryCriteria
        {
            BatchSize = 10,
            ConfigPRO = true,
            ConfigUAT = false,
            LastLogID = 0
        };

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.True(actual);
    }
}