namespace CRM.UnitTests.Criteria.ResultAmendment;

using CRM.Criteria.AmendmentNotiFB;
using Xunit;

public class GetAmendmentNotiAutoFeedbackInfoCriteriaTests
{
    [Fact]
    public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
    {
        // Arrange
        var criteria = new GetAmendmentNotiAutoFeedbackInfoCriteria();
        var expected = "Acc_CRM_AmendmentNoti_AutoFeedbackInfoSel";

        // Act
        var actual = criteria.GetSettingKey();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsValid_WhenCalled_ReturnTrue()
    {
        // Arrange
        var criteria = new GetAmendmentNotiAutoFeedbackInfoCriteria();

        // Act
        var actual = criteria.IsValid();

        // Assert
        Assert.True(actual);
    }
}