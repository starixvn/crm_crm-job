namespace CRM.UnitTests.Criteria.ChatBotProblemsAcc
{
    using CRM.Criteria.ChatbotProblemAccount;
    using Xunit;

    public class GetProblemsAccountByLogIdTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetProblemsAccountByLogId();
            var expected = "Acc_CRM_Chatbot_ProblemAccounts_Sel";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_ParamIsValid_ReturnTrue()
        {
            // Arrange
            var criteria = new GetProblemsAccountByLogId
            {
                LastLogId = 1,
                BatchSize = 10
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
