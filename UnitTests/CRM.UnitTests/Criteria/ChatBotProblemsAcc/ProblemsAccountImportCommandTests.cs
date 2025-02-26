namespace CRM.UnitTests.Criteria.ChatBotProblemsAcc
{
    using CRM.Criteria.ChatbotProblemAccount;
    using Xunit;

    public class ProblemsAccountImportCommandTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new ProblemsAccountImportCommand();
            var expected = "Acc_CRM_Chatbot_ProblemAccounts_Import";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnTrue()
        {
            // Arrange
            var criteria = new ProblemsAccountImportCommand
            {
                XmlData = "test",
                Environment = "LIVE",
                ErrCode = 0,
                ErrMsg = ""
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
        public void IsValid_ParamIsInvalid_ReturnFalse(string XmlData)
        {
            // Arrange
            var criteria = new ProblemsAccountImportCommand
            {
                XmlData = XmlData
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.False(actual);
        }
    }
}
