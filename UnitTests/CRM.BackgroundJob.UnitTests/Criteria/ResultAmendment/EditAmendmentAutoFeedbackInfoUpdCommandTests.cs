using CRM.BackgroundJob.Criteria.AmendmentNotiFB;
using Xunit;

namespace CRM.BackgroundJob.UnitTests.Criteria.ResultAmendment
{
    public class EditAmendmentAutoFeedbackInfoUpdCommandTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new EditAmendmentAutoFeedbackInfoUpdCommand();
            var expected = "Acc_CRM_AmendmentNoti_AutoFeedbackInfoUpd";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_ParamIsInvalid_ReturnTrue()
        {
            // Arrange
            var criteria = new EditAmendmentAutoFeedbackInfoUpdCommand
            {
                LogID = 1,
                Status = "1",
                Description = "test"
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }

        [Theory]
        [InlineData(2, null)]
        [InlineData(0, "")]
        [InlineData(1, "")]
        public void IsValid_ParamIsInvalid_ReturnFalse(int LogID, string Status)
        {
            // Arrange
            var criteria = new EditAmendmentAutoFeedbackInfoUpdCommand
            {
                LogID = LogID,
                Status = Status,
                Description = "test"
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.False(actual);
        }
    }
}