namespace CRM.BackgroundJob.UnitTests.Criteria.ProblemsAccount
{
    using CRM.BackgroundJob.Criteria.ProblemsAccount;
    using Xunit;

    public class InsertProblemsAccountAutoFeedbackInforCommandTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new InsertProblemsAccountAutoFeedbackInforCommand();
            var expected = "Acc_CRM_ProblemAccount_AutoFeedbackInfoIns";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_ParamIsInvalid_ReturnTrue()
        {
            // Arrange
            var criteria = new InsertProblemsAccountAutoFeedbackInforCommand
            {
                xml = "1"
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

        public void IsValid_ParamIsInvalid_ReturnFalse(string xml)
        {
            // Arrange
            var criteria = new InsertProblemsAccountAutoFeedbackInforCommand
            {
                xml = xml
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.False(actual);
        }
    }
}
