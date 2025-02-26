namespace CRM.UnitTests.Criteria.ProblemsAccount
{
    using CRM.Criteria.ProblemsAccount;
    using Xunit;

    public class EditProblemsAccountAutoFeedbackInfoCommandTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new EditProblemsAccountAutoFeedbackInfoCommand();
            var expected = "Acc_CRM_ProblemAccount_AutoFeedbackInfoUpd";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_ParamIsInvalid_ReturnTrue()
        {
            // Arrange
            var criteria = new EditProblemsAccountAutoFeedbackInfoCommand
            {
                LogID = "1",
                Status = "1",
                Description = "test"
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData(" ", " ")]
        [InlineData("1", "")]
        [InlineData(" ", "1")]
        public void IsValid_ParamIsInvalid_ReturnFalse(string LogID, string Status)
        {
            // Arrange
            var criteria = new EditProblemsAccountAutoFeedbackInfoCommand
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