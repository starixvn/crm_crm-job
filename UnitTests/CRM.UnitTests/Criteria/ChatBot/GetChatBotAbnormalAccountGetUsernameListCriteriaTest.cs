namespace CRM.UnitTests.Criteria.ChatBot;

using CRM.Criteria.ChatBot;
using Xunit;

public class GetChatBotAbnormalAccountGetUsernameListCriteriaTest
{
    [Fact]
    public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
    {
        // Arrange
        var criteria = new GetChatBotAbnormalAccountGetUsernameListCriteria();
        var expected = "Acc_CRM_ChatBot_AbnormalAccount_GetUsernameList";

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
        var criteria = new GetChatBotAbnormalAccountGetUsernameListCriteria
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
        var criteria = new GetChatBotAbnormalAccountGetUsernameListCriteria
        {
            Messages = "message"
        };

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.True(actual);
    }
}