
namespace CRM.BackgroundJob.UnitTests.Criteria.AmendmentNotiFB
{
    using CRM.BackgroundJob.Criteria.AmendmentNotiFB;
    using Xunit;
    public class GetAmendmentNotiAutoFeedbackInforCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetAmendmentNotiAutoFeedbackInforCriteria();
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
            var criteria = new GetAmendmentNotiAutoFeedbackInforCriteria();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
