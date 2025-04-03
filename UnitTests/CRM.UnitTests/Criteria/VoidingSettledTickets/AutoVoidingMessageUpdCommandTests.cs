namespace CRM.UnitTests.Criteria.VoidingSettledTickets;

using CRM.Criteria.VoidingSettledTickets;
using Xunit;

public class AutoVoidingMessageUpdCommandTests
{
    [Fact]
    public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
    {
        // Arrange
        var criteria = new AutoVoidingMessageUpdCommand();
        var expected = "Acc_CRM_AutoVoidingMessageUpd";

        // Act
        var actual = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsValid_WhenCalled_ReturnTrue()
    {
        // Arrange
        var criteria = new AutoVoidingMessageUpdCommand
        {
            MessageIds = "MessageIds",
            Environment = "LIVE"
        };

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.True(actual);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void IsValid_ParamIsInvalid_ReturnFalse(string messageIDs)
    {
        // Arrange
        var criteria = new AutoVoidingMessageUpdCommand
        {
            MessageIds = messageIDs
        };

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.False(actual);
    }
}