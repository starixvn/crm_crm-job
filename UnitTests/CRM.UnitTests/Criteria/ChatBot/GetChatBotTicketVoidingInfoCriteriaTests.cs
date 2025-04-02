namespace CRM.UnitTests.Criteria.ChatBot;

using CRM.Criteria.ChatBot;
using Xunit;

public class GetChatBotTicketVoidingInfoCriteriaTests
{
    [Fact]
    public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
    {
        // Arrange
        var criteria = new GetChatBotTicketVoidingInfoCriteria();
        var expected = "Acc_CRM_Chatbot_AbnormalBets_VoidingInfo_Sel";

        // Act
        var actual = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void IsValid_ParamIsInvalid_ReturnFalse(string messages)
    {
        // Arrange
        var criteria = new GetChatBotTicketVoidingInfoCriteria
        {
            Messages = messages
        };

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void IsValid_ParamIsValid_ReturnTrue()
    {
        // Arrange
        var criteria = new GetChatBotTicketVoidingInfoCriteria
        {
            Messages = "message",
            Environment = "Live",
            IncludeUUSCurrency = true
        };

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.True(actual);
    }
}