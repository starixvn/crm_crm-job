namespace CRM.UnitTests.Criteria.ChatBot_CTO;

using CRM.Criteria.ChatBot_CTO;
using Xunit;

public class GetTOControlSettingByLastLogIdCriteriaTests
{
    [Fact]
    public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
    {
        // Arrange
        var criteria = new GetTOControlSettingByLastLogIdCriteria();
        var expected = "Acc_Chatbot_TOControlSetting_Sel";

        // Act
        var actual = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsValid_ParamIsValid_ReturnTrue()
    {
        // Arrange
        var criteria = new GetTOControlSettingByLastLogIdCriteria();

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.True(actual);
    }
}