namespace CRM.BackgroundJob.UnitTests.Criteria.VoidingSettledTickets
{
    using CRM.BackgroundJob.Criteria.VoidingSettledTickets;
    using Xunit;
    public class GetVoidingAutoFeedbackInfoCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetVoidingAutoFeedbackInfoCriteria();
            var expected = "Acc_CRM_VoidingAutoFeedbackInfo_Sel";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnTrue()
        {
            // Arrange
            var criteria = new GetVoidingAutoFeedbackInfoCriteria();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
