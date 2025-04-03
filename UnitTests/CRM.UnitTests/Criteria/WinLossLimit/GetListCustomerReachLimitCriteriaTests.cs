namespace CRM.UnitTests.Criteria.WinLossLimit;

using CRM.Constants;
using CRM.Criteria.WinLossLimit;
using Xunit;

public class GetListCustomerReachLimitCriteriaTests
{
    [Fact]
    public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
    {
        // Arrange
        var criteria = new GetListCustomerReachLimitCriteria();
        var expected = "Acc_CRM_CustWinLimitSel";

        // Act
        var actual = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public void IsValid_ParamIsInvalid_ReturnFalse(int LastLogID ,int BatchSize)
    {
        // Arrange
        var criteria = new GetListCustomerReachLimitCriteria
        {
            LastLogID = LastLogID,
            BatchSize = BatchSize,
            Environment = BackgroundJobConstants.DefaultEnvironment
        };

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void IsValid_WhenCalled_ReturnTrue()
    {
        // Arrange
        var criteria = new GetListCustomerReachLimitCriteria
        {
            LastLogID = 1,
            BatchSize = 1,
            Environment = BackgroundJobConstants.DefaultEnvironment
        };

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.True(actual);
    }
}