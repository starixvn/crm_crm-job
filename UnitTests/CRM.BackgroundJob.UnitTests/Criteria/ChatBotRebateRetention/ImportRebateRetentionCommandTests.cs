namespace CRM.BackgroundJob.UnitTests.Criteria.ChatBotRebateRetention
{
    using CRM.BackgroundJob.Criteria.ChatBotRebateRetention;
    using Xunit;

    public class ImportRebateRetentionCommandTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new ImportRebateRetentionCommand();
            var expected = "Acc_CRM_Chatbot_RebateRetention_Import";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_ParamIsValid_ReturnTrue()
        {
            // Arrange
            var criteria = new ImportRebateRetentionCommand
            {
                XmlData = "data",
                NewLogSID = "1",
                Environment = "dev"
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}
