namespace CRM.UnitTests.Criteria.ResultAmendment;

using CRM.Criteria.AmendmentNotiFB;
using Xunit;

public class InsertAmendmentNotificationCommandTests
{
    [Fact]
    public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
    {
        // Arrange
        var criteria = new InsertAmendmentNotificationCommand();
        var expected = "Acc_CRM_AmendmentNoti_AutoFeedbackInfoIns";

        // Act
        var actual = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsValid_ParamIsInvalid_ReturnTrue()
    {
        // Arrange
        var criteria = new InsertAmendmentNotificationCommand
        {
            Xml = "1",
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
    public void IsValid_ParamIsInvalid_ReturnFalse(string Xml)
    {
        // Arrange
        var criteria = new InsertAmendmentNotificationCommand
        {
            Xml = Xml,
            Environment = "LIVE"
        };

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.False(actual);
    }
}