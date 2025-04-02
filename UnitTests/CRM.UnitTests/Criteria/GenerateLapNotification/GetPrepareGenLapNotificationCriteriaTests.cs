namespace CRM.UnitTests.Criteria.GenerateLapNotification;

using CRM.Criteria.GenerateLapNotification;
using Xunit;

public class GetPrepareGenLapNotificationCriteriaTests
{
    [Fact]
    public void GetSettingKey_ReturnCorrectSettingKey()
    {
        // Arrange
        var criteria = new GetPrepareGenLapNotificationCriteria();
        var expected = "MNC_ResultAmendment_PrepareGenLAP";

        // Action
        var actual = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsValid_ParamsAreValid_ReturnTrue()
    {
        // Arrange
        var criteria = new GetPrepareGenLapNotificationCriteria();

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.True(actual);
    }
}