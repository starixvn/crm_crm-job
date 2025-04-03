namespace CRM.UnitTests.Criteria.ChatBotResultAmendment;

using CRM.Criteria.ChatBot_ResultAmendment;
using Xunit;

public class GetChatbotVoidingMessageCheckTests
{
    [Fact]
    public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
    {
        // Arrange
        var criteria = new GetChatbotVoidingMessageCheck();
        var expected = "Acc_CRM_VodingMessage_Check";

        // Act
        var actual = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsValid_ParamIsValid_ReturnTrue()
    {
        // Arrange
        var criteria = new GetChatbotVoidingMessageCheck
        {
            Message = "message"
        };

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.True(actual);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]

    public void IsValid_ParamIsInvalid_ReturnFalse(string message)
    {
        // Arrange
        var criteria = new GetChatbotVoidingMessageCheck
        {
            Message = message
        };

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.False(actual);
    }
}