namespace CRM.UnitTests.Criteria.ChatBotCWL;

using CRM.Criteria.ChatBotCWL;
using Xunit;

public class GetCWLInfoByLogIdCriteriaTests
{
    [Fact]
    public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
    {
        // Arrange
        var criteria = new GetCWLInfoByLogIdCriteria();
        var expected = "Acc_CRM_Chatbot_CWL_Log_Sel";

        // Act
        var actual = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsValid_ParamIsValid_ReturnTrue()
    {
        // Arrange
        var criteria = new GetCWLInfoByLogIdCriteria
        {
            LastLogId = 1,
            BatchSize = 10
        };

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.True(actual);
    }
}