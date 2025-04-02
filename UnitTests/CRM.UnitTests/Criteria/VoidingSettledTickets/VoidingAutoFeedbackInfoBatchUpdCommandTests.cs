namespace CRM.UnitTests.Criteria.VoidingSettledTickets;

using CRM.Criteria.VoidingSettledTickets;
using Xunit;

public class VoidingAutoFeedbackInfoBatchUpdCommandTests
{
    [Fact]
    public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
    {
        // Arrange
        var criteria = new VoidingAutoFeedbackInfoBatchUpdCommand
        {
            LogVoidingID = 1,
            Status = "status",
            Description = "Description",
            Environment = "LIVE"
        };
        var expected = "Acc_CRM_VoidingAutoFeedbackInfo_BatchUpd";

        // Act
        var actual = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsValid_WhenCalled_ReturnTrue()
    {
        // Arrange
        var criteria = new VoidingAutoFeedbackInfoBatchUpdCommand
        {
            LogVoidingID = 1,
            Status = "status",
            Description = "Description",
            Environment = "LIVE"
        };

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.True(actual);
    }

    [Theory]
    [InlineData(1, null, null)]
    [InlineData(0, "", null)]
    [InlineData(1, " ", "")]
    public void IsValid_ParamIsInvalid_ReturnFalse(int logVoidingID, string status, string description)
    {
        // Arrange
        var criteria = new VoidingAutoFeedbackInfoBatchUpdCommand
        {
            LogVoidingID = logVoidingID,
            Status = status,
            Description = description
        };

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.False(actual);
    }
}