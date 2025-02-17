namespace CRM.BackgroundJob.UnitTests.Criteria.AutoFeedback
{
    using Fanex.CRM.BackgroundJob.Criteria;
    using Xunit;

    public class ImportFeedbackInfoCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new ImportFeedbackInfoCriteria();
            var expected = "Acc_CRM_AutoFeedbackInfoIns";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnTrue()
        {
            // Arrange
            var criteria = new ImportFeedbackInfoCriteria();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}