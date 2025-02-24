using CRM.BackgroundJob.Criteria.AmendmentNotiFB;
using Xunit;

namespace CRM.BackgroundJob.UnitTests.Criteria.ResultAmendment
{
    public class GetResultAmendmentSystemSettingCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetResultAmendmentSystemSettingCriteria();
            var expected = "Acc_CRM_AmendmentNoti_SystemSettingSel";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnTrue()
        {
            // Arrange
            var criteria = new GetResultAmendmentSystemSettingCriteria();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}