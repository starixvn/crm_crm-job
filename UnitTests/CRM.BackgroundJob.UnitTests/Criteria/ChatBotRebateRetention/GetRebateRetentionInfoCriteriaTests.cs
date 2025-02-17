namespace CRM.BackgroundJob.UnitTests.Criteria.ChatBotRebateRetention
{
    using CRM.BackgroundJob.Criteria.ChatBotRebateRetention;
    using Xunit;

    public class GetRebateRetentionInfoCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetRebateRetentionInfoCriteria();
            var expected = "Acc_CRM_Chatbot_RebateRetention_Sel";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_ParamIsValid_ReturnTrue()
        {
            // Arrange
            var criteria = new GetRebateRetentionInfoCriteria
            {
                LastLogSID = "1",
                NewLogSID = "1"
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
