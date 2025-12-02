namespace CRM.UnitTests.Criteria.ChatBotRebateRetention;

using CRM.Criteria.ChatBotRebateRetention;
using Xunit;

public class GetRebateRetentionInfoCriteriaTests
{
    [Fact]
    public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
    {
        // Arrange
        var criteria = new GetRebateRetentionInfoCriteria();
        var expected = "Acc_CRM_Chatbot_RebateRetention_Sel";

        // Act
        var actual = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsValid_ParamIsValid_ReturnTrue()
    {
        // Arrange
        var criteria = new GetRebateRetentionInfoCriteria
        {
            LastLogSID = "1",
            NewLogSID = "1"
        };

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Constructor_Assign_CorrectProperties()
    {
        // Arrange & Act
        var criteria = new GetRebateRetentionInfoCriteria
        {
            LastLogSID = "1",
            BatchSize = 50,
            NewLogSID = "2"
        };

        // Assert
        Assert.True("1" == criteria.LastLogSID && criteria.BatchSize == 50 && "2" == criteria.NewLogSID);
    }
}