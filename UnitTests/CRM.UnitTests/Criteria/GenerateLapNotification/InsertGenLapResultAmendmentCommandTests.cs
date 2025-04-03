namespace CRM.UnitTests.Criteria.GenerateLapNotification;

using CRM.Criteria.GenerateLapNotification;
using Xunit;

public class InsertGenLapResultAmendmentCommandTests
{
    [Fact]
    public void GetSettingKey_ReturnCorrectSettingKey()
    {
        // Arrange
        var command = new InsertGenLapResultAmendmentCommand();
        var expected = "MNC_ResultAmendment_GenNotificationLAP";

        // Action
        var actual = command.GetSettingKey();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsValid_ParamsAreValid_ReturnTrue()
    {
        // Arrange
        var command = new InsertGenLapResultAmendmentCommand();

        // Act
        var actual = command.IsValid();

        // Assert
        Assert.True(actual);
    }
}