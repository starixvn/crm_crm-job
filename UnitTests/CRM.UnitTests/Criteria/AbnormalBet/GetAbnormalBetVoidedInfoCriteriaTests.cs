namespace CRM.UnitTests.Criteria.AbnormalBet;

using CRM.Criteria.AbnormalBet;
using Xunit;

public class GetAbnormalBetVoidedInfoCriteriaTests
{
    [Fact]
    public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
    {
        // Arrange
        var criteria = new GetAbnormalBetVoidedInfoCriteria();
        var expected = "Acc_CRM_AutoFB_AbnormalBets_GetVoidedInfo";

        // Act
        var actual = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsValid_WhenCalled_ReturnTrue()
    {
        // Arrange
        var criteria = new GetAbnormalBetVoidedInfoCriteria
        {
            BatchSize = 2,
            MaxVoidingHistoryCollectionId = 2,
            LastVoidingHistoryCollectionId = 2,
        };

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void IsInvalid_WhenCalled_ReturnTrue()
    {
        // Arrange
        var criteria = new GetAbnormalBetVoidedInfoCriteria
        {
            BatchSize = 0,
            MaxVoidingHistoryCollectionId = 2,
            LastVoidingHistoryCollectionId = 2,
        };

        // Act
        var actual = criteria.IsInvalid();

        // Assert
        Assert.True(actual);
    }
}